using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.DTOs.ItemDTOs;
using WebApplication3.Helpers;
using WebApplication3.Middlewares;
using WebApplication3.Models;

 //PASS

namespace WebApplication3.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly FleetDB _context;

        public ItemController(FleetDB context)
        {
            _context = context; 
        }

        [JwtAuthMiddleware]
        [HttpPost("createItem")]
        public async Task<IActionResult> CreateItem([FromBody] CreateItemDTO itemDTO)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền này", Data = null });
                }
                var checkString = ValidateString.StringValidator(new Dictionary<string, string> 
                {
                    { "Tên hàng hóa" , itemDTO.ItemName } ,
                    { "Giá gốc" , itemDTO.FixedPrice.ToString() } ,
                    { "Sắp xếp DisplayOrder" , itemDTO.DisplayOrder.ToString() } ,
                });
                if (checkString != null)
                {
                    return Ok(checkString);
                }

                var existItem = await _context.Items.AnyAsync(i => i.ItemName ==  itemDTO.ItemName);
                if (existItem)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Trùng tên chi phí", Data = null });
                }
                var existDisplayOrder = await _context.Items.AnyAsync(i => i.DisplayOrder == itemDTO.DisplayOrder);
                if (existDisplayOrder)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Trùng thứ tự chi phí", Data = null });
                }


                Item newItem = new Item()
                {
                    ItemName = itemDTO.ItemName.ToUpper(),    
                    FixedPrice = itemDTO.FixedPrice,
                    DisplayOrder = itemDTO.DisplayOrder,
                };
                _context.Items.Add(newItem);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode  = 200,
                    Message = "Tạo chi phí thành công",
                    Data = newItem
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 400, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpPut("UpdateItem/{id}")]
        public async Task<IActionResult> UpdateItem( int id ,[FromBody] UpdateItemDTO itemDTO)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin.", Data = null });
                }
                var existItem = await _context.Items.FirstOrDefaultAsync(i => i.ItemID == id);
                if (existItem == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không có chi phí này", Data = null });
                }
                var validateData = ValidateString.StringValidator(new Dictionary<string, string>
                {
                    { "Tên hàng hóa" , itemDTO.ItemName } ,
                    { "Giá gốc" , itemDTO.FixedPrice.ToString() } ,
                    { "Sắp xếp DisplayOrder" , itemDTO.DisplayOrder.ToString() } ,
                });
                if (validateData != null)
                {
                    return Ok(validateData);
                }
                var existItemName = await _context.Items.AnyAsync(i => i.ItemName == itemDTO.ItemName && i.ItemID != id);
                if (existItemName)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Trùng tên chi phí", Data = null });
                }
                
                var existDisplayOrder = await _context.Items.AnyAsync(i => i.ItemID != id && i.DisplayOrder == itemDTO.DisplayOrder);
                if (existDisplayOrder)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Trùng thứ tự chi phí", Data = null });
                }
                existItem.ItemName = itemDTO.ItemName.ToUpper();
                existItem.FixedPrice = itemDTO.FixedPrice;
                existItem.DisplayOrder = itemDTO.DisplayOrder;

                _context.Items.Update(existItem);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Cập nhật chi phí thành công",
                    Data = existItem
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 400, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthMiddleware]
        [HttpGet("detailItem")]
        public async Task<IActionResult> DetailItem(
            [FromQuery] int id
        )
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin.", Data = null });
                }
                if (id == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Chưa có ID hàng hóa", Data = null });
                }
                var existItem = await _context.Items.FirstOrDefaultAsync(d => d.ItemID == id);
                if (existItem == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không có Hàng hóa này trong hệ thống", Data = null });
                }
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Thông tin hàng hóa '{existItem.ItemName}' ",
                    Data = existItem
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }

        }



        [JwtAuthMiddleware]
        [HttpGet("listItem")]
        public async Task<IActionResult> ListItem(
            [FromQuery] string order = "asc", // ✅ Default value
            [FromQuery] string sortBy = "id", // ✅ Default value  
            [FromQuery] int pageSize = 10,    // ✅ Default value
            [FromQuery] int pageNumber = 1,    // ✅ Default value
            [FromQuery] string searchKey = "" // ✅ Default value  

        )
        {
            try
            {
                //var adminOrEntry = FilterEntryOrAdmin.CheckEntryOrAdmin(HttpContext);
                //if (adminOrEntry != null)
                //{
                //    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin hoặc Entry.", Data = null });
                //}

                var query = _context.Items.Where(i => i.IsActive == true
                    && (string.IsNullOrEmpty(searchKey)
                        || i.ItemID.ToString().Contains(searchKey)
                        || i.ItemName.Contains(searchKey)
                ));  
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(i => i.ItemName)
                            : query.OrderBy(i => i.ItemName);
                        break;
                    case "displayorder":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(i => i.DisplayOrder)
                            : query.OrderBy(i => i.DisplayOrder);
                        break;
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(i => i.ItemID)
                            : query.OrderBy(i => i.ItemID);
                        break;
                }
                int page = Math.Max(1, pageNumber);
                int size = Math.Max(1, pageSize);

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);

                var listItems = await query
                    .Select(i => new { i.ItemName, i.ItemID, i.IsActive, i.DisplayOrder, i.FixedPrice })
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Danh sách thông tin chi phí ",
                    Data =  listItems 
                });

            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthMiddleware]
        [HttpGet("searchItem")]
        public async Task<IActionResult> SearchItem(
            [FromQuery] string searchKey,
            [FromQuery] string order,
            [FromQuery] string sortBy,
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber
        )
        {
            try
            {
                var adminOrEntry = FilterEntryOrAdmin.CheckEntryOrAdmin(HttpContext);
                if (adminOrEntry != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin hoặc Entry.", Data = null });
                }


                var query = _context.Items.Where(i => i.IsActive == true && (string.IsNullOrEmpty(searchKey) || i.ItemName.Contains(searchKey)));
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(i => i.ItemName)
                            : query.OrderBy(i => i.ItemName);
                        break;
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(i => i.ItemID)
                            : query.OrderBy(i => i.ItemID);
                        break;
                }

                var size = Math.Max(1, pageSize);
                var page = Math.Max(1, pageNumber);

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);

                var listItems = await query
                    .Select( i => new { i.ItemName , i.ItemID } )
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();
                if (listItems == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không có Hàng Hóa này trong hệ thống", Data = null });
                }
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Danh sách tìm kiếm Hàng Hóa ",
                    Data = listItems
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }

        }


        [JwtAuthMiddleware]
        [HttpDelete("deleteItem/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                var itemRecord = await _context.Items
                    .FirstOrDefaultAsync(i => i.ItemID == id);
                if (itemRecord == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Không tìm thấy Hàng Hóa có ID: {id}.", Data = null });
                }
                itemRecord.IsActive = false;

                _context.Items.Update(itemRecord);
                await _context.SaveChangesAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Xóa Hàng hóa có ID: {id} thành công",
                    Data = itemRecord
                });

            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


    }



}
