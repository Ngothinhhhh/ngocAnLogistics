using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Quic;
using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.DTOs.TruckDTOs;
using WebApplication3.Helpers;
using WebApplication3.Middlewares;
using WebApplication3.Models;

namespace WebApplication3.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class TruckController : ControllerBase
    {
        private readonly FleetDB _context;

        public TruckController(FleetDB context)
        {
            _context = context;     
        }

        [JwtAuthMiddleware]
        [HttpPost("createTruck")]
        public async Task<IActionResult> CreateTruck([FromBody] CreateTruckDTO truckDTO)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                var checkString = ValidateString.StringValidator(new Dictionary<string, string>
                {
                    { "Mã container" , truckDTO.TruckNo },
                });
                if (checkString != null)
                {
                    return Ok(checkString);
                }
                bool duplicateTruckNo = await _context.Trucks.AnyAsync(t => t.TruckNo == truckDTO.TruckNo);
                if (duplicateTruckNo)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Mã Container '{truckDTO.TruckNo}' đã tồn tại.", Data = null });
                }
                Truck newTruck = new Truck()
                {
                    TruckNo = truckDTO.TruckNo,
                    TruckType = 1
                };
                _context.Trucks.Add(newTruck);
                await _context.SaveChangesAsync();
               
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Tạo Container thành công",
                    Data = newTruck
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 400, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }

        [JwtAuthMiddleware]
        [HttpPost("createRmooc")]
        public async Task<IActionResult> CreateRmooc([FromBody] CreateTruckDTO truckDTO)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                var checkString = ValidateString.StringValidator(new Dictionary<string, string>
                {
                    { "Mã Rmooc" , truckDTO.TruckNo },
                });
                if (checkString != null)
                {
                    return Ok(checkString);
                }
                bool duplicateTruckNo = await _context.Trucks.AnyAsync(t => t.TruckNo == truckDTO.TruckNo);
                if (duplicateTruckNo)
                {
                    return Ok(new ApiResponse<string> { statusCode = 409, Message = $"⚠️ Mã Rmooc '{truckDTO.TruckNo}' đã tồn tại.", Data = null });
                }
                Truck newTruck = new Truck()
                {
                    TruckNo = truckDTO.TruckNo,
                    TruckType = 2
                };
                _context.Trucks.Add(newTruck);
                await _context.SaveChangesAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Tạo Rmooc thành công",
                    Data = newTruck
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }

        [JwtAuthMiddleware]
        [HttpPut("updateTruck/{id}")]
        public async Task<IActionResult> UpdateTruck([FromBody] UpdateTruckDTO truckDTO , int id)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                var truckRecord = await _context.Trucks.FirstOrDefaultAsync(t => t.TruckID == id && t.TruckType == 1);
                if (truckRecord == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️Không tìm thấy Container có ID : {id} .", Data = null });
                }

                var checkString = ValidateString.StringValidator(new Dictionary<string, string>
                {
                    { "Mã container" , truckDTO.TruckNo },
                    { "Trạng thái container" , truckDTO.isActive.ToString() },
                });
                if (checkString != null)
                {
                    return Ok(checkString);
                }
                var checkTruckNo = await _context.Trucks.AnyAsync(t => t.TruckNo == truckDTO.TruckNo && t.TruckID != id && t.TruckType == 1);
                if (checkTruckNo)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Mã Container '{truckDTO.TruckNo}' đã tồn tại.", Data = null });
                }

                truckRecord.TruckNo = truckDTO.TruckNo;
                truckRecord.IsActive = truckDTO.isActive;
                _context.Trucks.Update(truckRecord);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = "Cập nhật Container thành công",
                    data = truckRecord
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }

        [JwtAuthMiddleware]
        [HttpPut("updateRmooc/{id}")]
        public async Task<IActionResult> UpdateRmooc([FromBody] UpdateTruckDTO truckDTO, int id)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                var truckRecord = await _context.Trucks.FirstOrDefaultAsync(t => t.TruckID == id && t.TruckType == 2);
                if (truckRecord == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️Không tìm thấy Container có ID : {id} ", Data = null });
                }
                var checkString = ValidateString.StringValidator(new Dictionary<string, string>
                {
                    { "Mã Rmooc" , truckDTO.TruckNo },
                    { "Trạng thái Rmooc" , truckDTO.isActive.ToString() },
                });
                if (checkString != null)
                {
                    return Ok(checkString);
                }
                var checkTruckNo = await _context.Trucks.AnyAsync(t => t.TruckNo == truckDTO.TruckNo && t.TruckID != id && t.TruckType == 2);
                if (checkTruckNo)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Mã Container '{truckDTO.TruckNo}' đã tồn tại.", Data = null });
                }
                truckRecord.TruckNo = truckDTO.TruckNo;
                truckRecord.IsActive = truckDTO.isActive;
                _context.Trucks.Update(truckRecord);
                await _context.SaveChangesAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Cập nhật Rmooc thành công",
                    Data = truckRecord
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpGet("listTruck")]
        public async Task<IActionResult> ListTruck(
            [FromQuery] string sortBy,
            [FromQuery] string order,
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber
        )
        {
            try
            {
                //var adminOrEntry = FilterEntryOrAdmin.CheckEntryOrAdmin(HttpContext);
                //if (adminOrEntry != null)
                //{
                //    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin hoặc Entry.", Data = null });
                //}


                var query =  _context.Trucks.AsQueryable();
                switch (sortBy.ToLower())
                {
                    case "truckNo":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(t => t.TruckNo)
                            : query.OrderBy(t => t.TruckNo);
                        break;
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(t => t.TruckID)
                            : query.OrderBy(t => t.TruckID);
                        break;
                }

                var size = Math.Max(1, pageSize);
                var page = Math.Max(1, pageNumber);
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);

                var listTruck = await query
                    .Where(t => t.TruckType == 1)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .Select(t => new { t.TruckNo, t.TruckID })
                    .ToListAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Lấy danh sách Container thành công",
                    Data = new {listTruck,totalPages,totalItems}
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpGet("listRmooc")]
        public async Task<IActionResult> ListRmooc(
            [FromQuery] string sortBy,
            [FromQuery] string order,
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber
        )
        {
            try
            {
                //var adminOrEntry = FilterEntryOrAdmin.CheckEntryOrAdmin(HttpContext);
                //if (adminOrEntry != null)
                //{
                //    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin hoặc Entry.", Data = null });
                //}

                var query = _context.Trucks.AsQueryable();
                switch (sortBy.ToLower())
                {
                    case "truckNo":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(t => t.TruckNo)
                            : query.OrderBy(t => t.TruckNo);
                        break;
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(t => t.TruckID)
                            : query.OrderBy(t => t.TruckID);
                        break;
                }

                var size = Math.Max(1, pageSize);
                var page = Math.Max(1, pageNumber);
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);

                var listRmooc = await query
                    .Where(t => t.TruckType == 2)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .Select(t => new { t.TruckNo, t.TruckID })
                    .ToListAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Lấy danh sách Rmooc thành công",
                    Data = new { listRmooc, totalPages, totalItems }
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpDelete("deleteTruck/{id}")]
        public async Task<IActionResult> DeleteTruck(int id)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                var truckRecord = await _context.Trucks
                    .FirstOrDefaultAsync(t => t.TruckID == id && t.TruckType == 1);
                if (truckRecord == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Không tìm thấy Container có ID: {id}.", Data = null });
                }
                truckRecord.IsActive = false;
                _context.Trucks.Update(truckRecord);
                await _context.SaveChangesAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Xóa Container thành công",
                    Data =  truckRecord
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpDelete("deleteRmooc/{id}")]
        public async Task<IActionResult> DeleteRmooc(int id)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                var truckRecord = await _context.Trucks
                    .FirstOrDefaultAsync(t => t.TruckID == id && t.TruckType == 2);
                if (truckRecord == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Không tìm thấy Rmooc có ID: {id}.", Data = null });
                }
                truckRecord.IsActive = false;
                _context.Trucks.Update(truckRecord);
                await _context.SaveChangesAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Xóa Rmooc thành công",
                    Data = truckRecord
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }




        [JwtAuthMiddleware]
        [HttpGet("searchTruck")]
        public async Task<IActionResult> SearchTruck(
            [FromQuery] string order = "asc", // ✅ Default value
            [FromQuery] string sortBy = "id", // ✅ Default value  
            [FromQuery] int pageSize = 30,    // ✅ Default value
            [FromQuery] int pageNumber = 1,    // ✅ Default value
            [FromQuery] string keySearch = "" // ✅ Default value  
        )
        {
            try
            {
                //var adminOrEntry = FilterEntryOrAdmin.CheckEntryOrAdmin(HttpContext);
                //if (adminOrEntry != null)
                //{
                //    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin hoặc Entry.", Data = null });
                //}

                var query = _context.Trucks.Where(t => t.TruckType == 1 && (string.IsNullOrEmpty(keySearch) || t.TruckNo.Contains(keySearch) ));

                switch (sortBy.ToLower())
                {
                    case "truckNo":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(t => t.TruckNo)
                            : query.OrderBy(t => t.TruckNo);
                        break;
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(t => t.TruckID)
                            : query.OrderBy(t => t.TruckID);
                        break;
                }

                var size = Math.Max(1, pageSize);
                var page = Math.Max(1, pageNumber);
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);

                var listTruck = await query
                    .Select(t => new { t.TruckNo, t.TruckID, t.IsActive })
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Tìm kiếm danh sách Container thành công",
                    Data = listTruck
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpGet("searchRmooc")]
        public async Task<IActionResult> SearchRmooc(
            [FromQuery] string order = "asc", // ✅ Default value
            [FromQuery] string sortBy = "id", // ✅ Default value  
            [FromQuery] int pageSize = 30,    // ✅ Default value
            [FromQuery] int pageNumber = 1,    // ✅ Default value
            [FromQuery] string keySearch = "" // ✅ Default value  
        )
        {
            try
            {
                //var adminOrEntry = FilterEntryOrAdmin.CheckEntryOrAdmin(HttpContext);
                //if (adminOrEntry != null)
                //{
                //    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin hoặc Entry.", Data = null });
                //}

                var query = _context.Trucks.Where(t => t.TruckType == 2 && (string.IsNullOrEmpty(keySearch) || t.TruckNo.Contains(keySearch)));

                switch (sortBy.ToLower())
                {
                    case "truckNo":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(t => t.TruckNo)
                            : query.OrderBy(t => t.TruckNo);
                        break;
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(t => t.TruckID)
                            : query.OrderBy(t => t.TruckID);
                        break;
                }

                var size = Math.Max(1, pageSize);
                var page = Math.Max(1, pageNumber);
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);

                var listTruck = await query
                    .Select(t => new { t.TruckNo, t.TruckID, t.IsActive })
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Tìm kiếm danh sách Rmooc thành công",
                    Data = listTruck
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthMiddleware]
        [HttpGet("detailTruck")]
        public async Task<IActionResult> DetailTruck([FromQuery] int id)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                var truckRecord = await _context.Trucks.FirstOrDefaultAsync(t => t.TruckID == id && t.TruckType == 1);
                if (truckRecord == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️Không tìm thấy Container có ID : {id} .", Data = null });
                }
                 
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Thông tin chi tiết Container có ID : '{id}' ",
                    Data = truckRecord
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthMiddleware]
        [HttpGet("detailRmooc")]
        public async Task<IActionResult> DetailRmooc([FromQuery]int id)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                var truckRecord = await _context.Trucks.FirstOrDefaultAsync(t => t.TruckType == 2 && t.TruckID == id);
                if (truckRecord == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️Không tìm thấy Rmooc có ID : {id} .", Data = null });
                }
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Thông tin chi tiết Rmooc có ID : '{id}' ",
                    Data = truckRecord
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


    }


}
