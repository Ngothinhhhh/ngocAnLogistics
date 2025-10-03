using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.DTOs.DriverDTOs;
using WebApplication3.Helpers;
using WebApplication3.Middlewares;
using WebApplication3.Models;

namespace WebApplication3.Controllers.Admin                                                     //*********************
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly FleetDB _context;

        public DriverController(FleetDB context)
        {
            _context = context;
        }

        [JwtAuthMiddleware]
        [HttpPost("createDriver")]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDTO request)
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin.", Data = null });
                }              
                var validate = ValidateString.StringValidator(new Dictionary<string, string> 
                {

                    { "Tên tài xế "    , request.DriverName },
                    { "Địa chỉ tài xế" , request.Address  },
                    { "Số điện thoại"  , request.Phone },
                    { "Mã số bằng lái xe" , request.LicenseNo },
                    { "Ngày hết hạn"   , request.ExpireDate.ToString() } ,
                    { "Tên đăng nhập" , request.UserName },
                    { "Mật khẩu" , request.Password },
                    { "Họ và tên" , request.FullName },

                });
                if (validate != null)
                {
                    return Ok(validate);
                }
                bool UserNameExist = await _context.User.AnyAsync(u => u.UserName == request.UserName);
                if (UserNameExist)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Username '{request.UserName}' đã được sử dụng.", Data = null });
                }
                var checkLicenseNo = await _context.Drivers.AnyAsync(d => d.LicenseNo == request.LicenseNo);
                if (checkLicenseNo)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Trùng mã số bằng lái xe. Hãy nhập lại.", Data = null });
                }
                if (DateTime.Parse(request.ExpireDate) <= DateTime.Now)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Ngày hết hạn phải lớn hơn ngày hiện tại.", Data = null });
                }

                var roleid = await _context.Roles
                    .Where(r => r.RoleName == "Driver")
                    .Select(r => r.RoleID )
                    .FirstOrDefaultAsync();

                if (roleid == 0)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không tìm thấy RoleID này.", Data = null });
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var newUser = new User
                {
                    UserName = request.UserName,
                    FullName = request.FullName.ToUpper(),
                    Password = hashedPassword,
                    RoleID = roleid,
                };
                _context.User.Add(newUser);
                await _context.SaveChangesAsync();


                Driver newDriver = new Driver
                {
                    DriverName = request.DriverName.ToUpper(),
                    RoleID = roleid,
                    UserID = newUser.UserID,
                    Address = request.Address,
                    Phone = request.Phone,    
                    LicenseNo = request.LicenseNo,
                    ExpireDate = request.ExpireDate.ToString()
                };    

                _context.Drivers.Add(newDriver);
                await _context.SaveChangesAsync();
                //COMMIT 
                await transaction.CommitAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Tạo mới tài xế' {newDriver.DriverName} ' thành công",
                    Data = newDriver
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();    
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpPut("updateDriver/{id}")]
        public async Task<IActionResult> UpdateDriver(int id , [FromBody] UpdateDriverDTO driverDTO)
        {
            var transaction =  await _context.Database.BeginTransactionAsync(); 
            try
            {
                //var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                //if (checkAdmin != null)
                //{
                //    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin.", Data = null });
                //}
                var existDriver = await _context.Drivers
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DriverID == id);
                if (existDriver == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không có Người dùng này trong hệ thống", Data = null });
                }
                var validate = ValidateString.StringValidator(new Dictionary<string, string>
                {
                    { "Mật khẩu" , driverDTO.Password },
                    { "Tên tài xế "    , driverDTO.DriverName },
                    { "Địa chỉ tài xế" , driverDTO.Address  },
                    { "Số điện thoại"  , driverDTO.Phone },
                    { "Mã số bằng lái xe" , driverDTO.LicenseNo },
                    { "Ngày hết hạn"   , driverDTO.ExpireDate } ,
                    { "Trạng thái hoạt động của tài xế" , driverDTO.IsActive.ToString() },
                    { "Trạng thái của tài xế" , driverDTO.Status },
                });
                if (validate != null)
                {
                    return Ok(validate);
                }
                var checkLicenseNo = await _context.Drivers.AnyAsync(d => d.LicenseNo == driverDTO.LicenseNo && d.DriverID != id);
                if (checkLicenseNo)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Trùng mã số bằng lái xe. Hãy nhập lại", Data = null });
                }
                if (!DateTime.TryParse(driverDTO.ExpireDate, out var expireDate))
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Định dạng ngày không hợp lệ.", Data = null });
                }

                if (expireDate <= DateTime.Now)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Ngày hết hạn phải lớn hơn ngày hiện tại.", Data = null });
                }

                Enum.TryParse<Driver.StatusDriver>(driverDTO.Status,out var status);

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(driverDTO.Password);

                existDriver.User.Password = hashedPassword;
                existDriver.User.IsActive = driverDTO.IsActive;
                existDriver.DriverName = driverDTO.DriverName.ToUpper();
                existDriver.Address = driverDTO.Address.ToUpper();
                existDriver.Phone = driverDTO.Phone;
                existDriver.LicenseNo = driverDTO.LicenseNo;
                existDriver.ExpireDate = driverDTO.ExpireDate;
                existDriver.IsActive = driverDTO.IsActive;
                existDriver.Status = status;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Cập nhật Tài xế {existDriver.DriverName} thành công",
                    Data = existDriver
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpGet("detailDriver")]
        public async Task<IActionResult> detailDriver(
            [FromQuery] int id
        )
        {
            try
            {
                var adminOrEntry = FilterEntryOrAdmin.CheckEntryOrAdmin(HttpContext);
                if (adminOrEntry != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin hoặc Entry.", Data = null });
                }
                if (id == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Chưa có ID người dùng", Data = null });
                }
                var existDriver = await _context.Drivers
                    .Where(d => d.DriverID == id)
                    .Include( u => u.User)
                    .FirstOrDefaultAsync();
                if (existDriver == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không có Người dùng này trong hệ thống", Data = null });
                }

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Thông tin Tài xế '{existDriver.DriverName}' ",
                    Data = existDriver
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }

        }


        [JwtAuthMiddleware]
        [HttpGet("listDriver")]
        public async Task<IActionResult> listDriver(
                [FromQuery] string order = "asc", // ✅ Default value
                [FromQuery] string sortBy = "id", // ✅ Default value  
                [FromQuery] int pageSize = 10,    // ✅ Default value
                [FromQuery] int pageNumber = 1,    // ✅ Default value
                [FromQuery] string keySearch = ""    // ✅ Default value

        )
        {
            try
            {
                //var adminOrEntry = FilterEntryOrAdmin.CheckEntryOrAdmin(HttpContext);
                //if (adminOrEntry != null)
                //{
                //    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin hoặc Entry.", Data = null });
                //}

                keySearch.ToUpper();

                var query = _context.Drivers.Where(d => d.IsActive == true
                    && (string.IsNullOrEmpty(keySearch)
                        || d.DriverID.ToString().Contains(keySearch)
                        || d.DriverName.Contains(keySearch)
                        || d.Phone.Contains(keySearch)
                        || d.LicenseNo.Contains(keySearch)
                    ));
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(d => d.DriverName)
                            : query.OrderBy(d => d.DriverName);
                        break;
                    default:
                        query = order.ToLower() ==  "desc"
                            ? query.OrderByDescending(d => d.DriverID)
                            : query.OrderBy(d => d.DriverID); 
                        break;
                }
                int page = Math.Max(1, pageNumber);
                int size = Math.Max(1, pageSize);

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);


                var listDriver = await query
                    .Select(d => new { d.DriverID,d.LicenseNo, d.DriverName, d.IsActive ,d.Status })
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Danh sách thông tin tài xế ",
                    Data = listDriver 
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthMiddleware]
        [HttpGet("searchDriver")]
        public async Task<IActionResult> SearchDriver(
                [FromQuery] string searchKey,
                [FromQuery] string order = "asc", // ✅ Default value
                [FromQuery] string sortBy = "id", // ✅ Default value  
                [FromQuery] int pageSize = 10,    // ✅ Default value
                [FromQuery] int pageNumber = 1    // ✅ Default value
        )
        {
            try
            {
                var adminOrEntry = FilterEntryOrAdmin.CheckEntryOrAdmin(HttpContext);
                if (adminOrEntry != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin hoặc Entry.", Data = null });
                }

                var query = _context.Drivers.Where(d => d.IsActive == true && ( string.IsNullOrEmpty(searchKey) || d.DriverName.Contains(searchKey)) );
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(d => d.DriverName)
                            : query.OrderBy(d => d.DriverName);
                        break;
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(d => d.DriverID)
                            : query.OrderBy(d => d.DriverID);
                        break;
                }

                var size = Math.Max(1, pageSize);
                var page = Math.Max(1 , pageNumber);  

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);

                var listDriver = await query
                    .Select(d => new { d.DriverName, d.DriverID })
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();
                if (listDriver == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không có Người dùng này trong hệ thống", Data = null });
                }
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Danh sách tìm kiếm khách hàng ",
                    Data = listDriver
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }

        }
    }
}
