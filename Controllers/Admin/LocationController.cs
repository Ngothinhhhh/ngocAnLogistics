using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.DTOs.ItemDTOs;
using WebApplication3.DTOs.LocationDTOs;
using WebApplication3.Helpers;
using WebApplication3.Middlewares;
using WebApplication3.Models;

namespace WebApplication3.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly FleetDB _context;
        public LocationController(FleetDB context)
        {
            _context = context;
        }


        [JwtAuthMiddleware]
        [HttpPost("createLocation")]
        public async Task<IActionResult> CreateLocation([FromBody] CreateLocationDTO locationDTO)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                var validateData = ValidateString.StringValidator(new Dictionary<string, string>
                {
                    { "Tên Địa chỉ" , locationDTO.Locationname } ,
                });
                if (validateData != null)
                {
                    return Ok(validateData);
                }
                var existLocation = await _context.Locations.AnyAsync(l => l.LocationName == locationDTO.Locationname);
                if (existLocation)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Trùng tên địa chỉ", Data = null });
                }

                Location newLocation = new Location()
                {
                    LocationName = locationDTO.Locationname.ToUpper(),    
                };
                _context.Locations.Add(newLocation);
                await _context.SaveChangesAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Tạo Địa chỉ thành công",
                    Data = newLocation
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpPut("updateLocation/{id}")]
        public async Task<IActionResult> UpdateLocation(int id , [FromBody] UpdateLocation locationDTO)
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
                    { "Tên Địa chỉ" , locationDTO.Locationname } ,
                    { "Trạng thái Địa chỉ" , locationDTO.isActive.ToString() } ,
                });
                if (checkString != null)
                {
                    return Ok(checkString);
                }

                var existLocation = await _context.Locations.FirstOrDefaultAsync(l => l.LocationId == id);
                if (existLocation == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không có địa chỉ này", Data = null });
                }

                existLocation.LocationName = locationDTO.Locationname.ToUpper();
                existLocation.IsActive = locationDTO.isActive;
                _context.Locations.Update(existLocation);
                await _context.SaveChangesAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Cập nhật Địa chỉ thành công",
                    Data = existLocation
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthMiddleware]
        [HttpGet("detailLocation")]
        public async Task<IActionResult> DetailLocation(
            [FromQuery] int id
        )
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                if (id == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Chưa có ID Địa chỉ.", Data = null });
                }
                var existLocation = await _context.Locations.FirstOrDefaultAsync(l => l.LocationId == id);
                if (existLocation == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không có Địa chỉ này trong hệ thống.", Data = null });
                }
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Thông tin địa chỉ '{existLocation.LocationName}' ",
                    Data = existLocation
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }

        }



        [JwtAuthMiddleware]
        [HttpGet("listLocation")]
        public async Task<IActionResult> ListLocation(
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

                var query = _context.Locations.Where(l => l.IsActive == true && ( string.IsNullOrEmpty(searchKey) || l.LocationName.Contains(searchKey) || l.LocationId.ToString().Contains(searchKey) ));
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(l => l.LocationName)
                            : query.OrderBy(l => l.LocationName);
                        break;
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(l => l.LocationId)
                            : query.OrderBy(l => l.LocationId);
                        break;
                }
                int page = Math.Max(1, pageNumber);
                int size = Math.Max(1, pageSize);

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);


                var listLocation = await query
                    .Select(l => new { l.LocationId , l.LocationName, l.IsActive })
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Danh sách thông tin Địa chỉ ",
                    Data = new { listLocation , totalItems , totalPages }
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthMiddleware]
        [HttpGet("searchLocation")]
        public async Task<IActionResult> SearchLocation(
            [FromQuery] string order = "asc", // ✅ Default value
            [FromQuery] string sortBy = "id", // ✅ Default value  
            [FromQuery] int pageSize = 10,    // ✅ Default value
            [FromQuery] int pageNumber = 1 ,   // ✅ Default value
            [FromQuery] string searchKey = "" // ✅ Default value  
        )
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }

                var query = _context.Locations.Where(l => l.IsActive == true && (string.IsNullOrEmpty(searchKey) || l.LocationName.Contains(searchKey)));
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(l => l.LocationName)
                            : query.OrderBy(l => l.LocationName);
                        break;
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(l => l.LocationId)
                            : query.OrderBy(l => l.LocationId);
                        break;
                }

                var size = Math.Max(1, pageSize);
                var page = Math.Max(1, pageNumber);

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);

                var listLocation = await query
                    .Select(l => new { l.LocationId , l.LocationName})
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();
                if (listLocation == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không có Địa chỉ này trong hệ thống.", Data = null });
                }
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Danh sách tìm kiếm Địa chỉ ",
                    Data = listLocation
                });

            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }

        }


        [JwtAuthMiddleware]
        [HttpDelete("deleteLocation/{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                var locationRecord = await _context.Locations
                    .FirstOrDefaultAsync(l => l.LocationId == id);
                if (locationRecord == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Không tìm thấy Địa chỉ có ID: {id}.", Data = null });
                }

                locationRecord.IsActive = false;

                _context.Locations.Update(locationRecord);
                await _context.SaveChangesAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Xóa Địa chỉ có ID: {id} thành công",
                    Data = locationRecord
                });

            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }




    }
}
