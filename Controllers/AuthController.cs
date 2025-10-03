using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.DTOs.ImageDTOs;
using WebApplication3.DTOs.UserDTO;
using WebApplication3.Helpers;
using WebApplication3.Middlewares;
using WebApplication3.Models;
using WebApplication3.Services;
using WebApplication3.Services.DataAccessLayer.EfCore;
using WebApplication3.Services.Interfaces.EntityInterfaces;
using WebApplication3.Services.Services.Customers;
using WebApplication3.Services.Services.Photos;


namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly FleetDB _context;
        private readonly JwtService _jwtService;
        private readonly ICustomerService _customerService;
        // for uploads image
        private readonly IPhotoRepository _photoService;
        private readonly IImageService _imageService;    
        //private readonly ICustomerDal _customerDal;
        //public AuthController(FleetDB context , JwtService jwtService, ICustomerDal customerDal)
        //{
        //    _context = context; 
        //    _jwtService = jwtService;
        //    _customerDal = customerDal; 
        //}
        public AuthController(FleetDB context, JwtService jwtService, ICustomerService customerService, IPhotoRepository photoService , IImageService imageService)
        {
            _context = context;
            _jwtService = jwtService;
            _customerService = customerService;
            _photoService = photoService;
            _imageService = imageService;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthDTOs user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                {
                    return Ok(new ApiResponse<string>{ statusCode = 400,  Message = "⚠️ Username hoặc Password không được để trống.", Data = null });
                }

                var dbUser = await _context.User
                    .Include(u => u.Role)
                    .Where(u => u.UserName == user.Username && u.IsActive)
                    .Select(u => new
                    {
                        u.UserID,
                        u.UserName,
                        u.Password,
                        u.FullName,
                        RoleName = u.Role.RoleName,
                        u.RoleID,
                    })
                    .FirstOrDefaultAsync();

                if (dbUser == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ User không tồn tại", Data = null });
                }

                bool isValid = BCrypt.Net.BCrypt.Verify(user.Password, dbUser.Password);
                if (!isValid)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ Password không đúng", Data = null });
                }

                var claimsDTO = new UserClaimsDTO
                {
                    RoleName = dbUser.RoleName,
                    UserID = dbUser.UserID,
                    UserName = dbUser.UserName,
                    FullName = dbUser.FullName,
                    RoleID = dbUser.RoleID
                };

                string token = _jwtService.GenerateToken(claimsDTO);

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "✅ Đăng nhập thành công",
                    Data = new
                    {
                        token = token,
                        RoleName = dbUser.RoleName
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = "❌ Lỗi server", Data = null });
            }
        }


        //[JwtAuthMiddleware]
        [HttpPost("createNewUser")]
        public async Task<IActionResult> createNewUser([FromBody]NewUserDTO userDTO)
        {
            try
            {
                //var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                //if (checkAdmin != null)
                //{
                //    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                //}
                var checkString = ValidateString.StringValidator(new Dictionary<string, string>
                {
                    { "Tên đăng nhập" , userDTO.UserName },
                    { "Mật khẩu" , userDTO.Password },
                    { "Họ và tên" , userDTO.FullName },
                    { "RoleID  " , userDTO.RoleID.ToString() },
                });
                if (checkString != null)
                {
                    return Ok(checkString);
                }

                bool UserNameExist = await _context.User.AnyAsync(u => u.UserName == userDTO.UserName);
                if (UserNameExist)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Username '{userDTO.UserName}' đã được sử dụng.", Data = null });
                }
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);
                var newUser = new User
                {
                    UserName = userDTO.UserName,
                    FullName = userDTO.FullName,
                    Password = hashedPassword,
                    RoleID = userDTO.RoleID,
                };

                _context.User.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "✅ Tạo User thành công",
                    Data = newUser.UserID
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = "❌ Lỗi server", Data = null });
            }
        }


        [JwtAuthMiddleware]   //gọi constructor mặc định JwtAuthMiddleware (), và không dùng DI container để truyền các service vào.
        [HttpGet("getAllRole")]
        public async Task<IActionResult> getListRole()
        {
            try
            {
                var listRole = await _context.Roles.ToListAsync();
                return Ok(new ApiResponse<Object> { statusCode = 200, Message = $"Danh sách quyền", Data = listRole });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"❌ Lỗi: {ex.Message}", Data = null });
            }
        }


        [HttpPost("createNewRoleByAdmin")]
        public async Task<IActionResult> createRole(CreateRoleDTO roleDTO)
        {
            try
            {
                // 1. Validate dữ liệu
                if (string.IsNullOrWhiteSpace(roleDTO.RoleName))
                {
                    return BadRequest("⚠️ RoleName không được để trống.");
                }

                // 2. Kiểm tra trùng tên
                bool isExist = await _context.Roles.AnyAsync(r => r.RoleName == roleDTO.RoleName);
                if (isExist)
                {
                    return Conflict($"⚠️ Role '{roleDTO.RoleName}' đã tồn tại.");
                }

                // 3. Thêm role mới
                var role = new Role()
                {
                    RoleName = roleDTO.RoleName,
                };

                _context.Roles.Add(role);               // Đưa vào bộ nhớ EF (state: Added)
                await _context.SaveChangesAsync();      // INSERT INTO Roles (RoleName) VALUES ('Admin')

                return Ok("oke");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Lỗi: {ex.Message}");
            }
        }



        [JwtAuthMiddleware]
        [HttpPost("createNewUserRoleByAdmin")]
        public async Task<IActionResult> CreateNewUserRoleByAdmin([FromBody] NewUserDTO userDTO)
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
                    { "Tên đăng nhập" , userDTO.UserName },
                    { "Mật khẩu" , userDTO.Password },
                    { "Họ và tên" , userDTO.FullName },
                    { "RoleID  " , userDTO.RoleID.ToString() },
                });
                if (checkString != null)
                {
                    return Ok(checkString);
                }

                bool UserNameExist = await _context.User.AnyAsync(u => u.UserName == userDTO.UserName);
                if (UserNameExist)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Username '{userDTO.UserName}' đã được sử dụng.", Data = null });
                }
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);
                var newUser = new User
                {
                    UserName = userDTO.UserName,
                    FullName = userDTO.FullName,
                    Password = hashedPassword,
                    RoleID = userDTO.RoleID,
                };

                _context.User.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "✅ Tạo User thành công",
                    Data = newUser
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = "❌ Lỗi server", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpGet("listUser")]
        public async Task<IActionResult> ListUser(
                [FromQuery] string order = "asc", // ✅ Default value
                [FromQuery] string sortBy = "id", // ✅ Default value  
                [FromQuery] int pageSize = 30,    // ✅ Default value
                [FromQuery] string keySearch = "",    // ✅ Default value
                [FromQuery] int pageNumber = 1,    // ✅ Default value
                [FromQuery] int roleID = 0          // ✅ Default value
        )
        {
            try
            {
                var query = _context.User
                    .Where(c => c.IsActive == true
                        && (string.IsNullOrEmpty(keySearch)
                        || c.UserName.Contains(keySearch)
                        || c.UserID.ToString().Contains(keySearch)
                        || c.FullName.Contains(keySearch)
                ));

                switch (sortBy.ToLower())
                {
                    case "name":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(c => c.UserName)
                            : query.OrderBy(c => c.UserName);
                        break;

                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(c => c.UserID)
                            : query.OrderBy(c => c.UserID);
                        break;
                }

                if (roleID != 0)
                {
                    query = query.Where(u => u.RoleID == roleID) ;
                }

                // Phân trang
                int page = Math.Max(1, pageNumber); // tránh page < 1
                int size = Math.Max(1, pageSize);   // tránh size < 1

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);

                var listUser = await query
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Lấy danh sách người dùng thành công",
                    Data = listUser
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }

        [JwtAuthMiddleware]
        [HttpPut("updateUserRoleByAdmin")]
        public async Task<IActionResult> UpdateUserRoleByAdmin(
            [FromBody] UpdateUserDTO userDTO,
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
                var checkString = ValidateString.StringValidator(new Dictionary<string, string>
                {
                    { "Mật khẩu" , userDTO.Password },
                    { "Họ và tên" , userDTO.FullName },
                    { "RoleID  " , userDTO.RoleID.ToString() },
                });
                if (checkString != null)
                {
                    return Ok(checkString);
                }

                var userExist = await _context.User.FirstOrDefaultAsync(u => u.UserID == id);
                if (userExist == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ Không tìm thấy Người dùng này", Data = null });
                }
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);

                userExist.FullName = userDTO.FullName;
                userExist.Password = hashedPassword;
                userExist.RoleID = userDTO.RoleID;
                

                _context.User.Update(userExist);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "✅ Cập nhật User thành công",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = "❌ Lỗi server", Data = null });
            }
        }



        [JwtAuthMiddleware]
        [HttpGet("detailUser")]
        public async Task<IActionResult> DetailUser(
            [FromQuery] int id
        )
        {
            try
            {
                if (id == null || id <= 0)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = ID User không hợp lệ.", Data = null });
                }

                var detailUser = await _context.User
                    .Where(c => c.UserID == id && c.IsActive == true)
                    .FirstOrDefaultAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Thông tin chi tiết người dùng '{detailUser.UserName}' thành công.",
                    Data = detailUser
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [HttpGet("testRepository")]
        public async Task<IActionResult> TestRepository(
        )
        {
            try
            {
                var detail = await  _customerService.GetActiveCustomers();
                //var detail = await _customerDal.GetCustomersWithOrders();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Thông tin thành công.",
                    Data = detail
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [HttpPost("testRepository2")]
        public async Task<IActionResult> TestRepository2(
            int userId, // sẽ thay đổi thành lấy dựa trên token
            [FromForm] PhotoForCreationDTO photoForCreationDTO
        )
        {
            try
            {
                //var profileImage = await _photoService.AddPhotoForUserAsync(userId, photoForCreationDTO);
                var result = await _imageService.AddImageAsync(userId ,photoForCreationDTO);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



















        //[HttpPost("createToken")]
        //public async Task<IActionResult> createToken(UserClaimsDTO userClaims)
        //{
        //    try
        //    {
        //        var stringToken = _jwtService.GenerateToken(userClaims);
        //        return Ok(stringToken);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"❌ Lỗi: {ex.Message}");
        //    }
        //}

        //[HttpGet("decodedToken")]
        //public async Task<IActionResult> decodedToken()
        //{
        //    try
        //    {
        //        string authHeader = Request.Headers["Authorization"];
        //        string token = null;

        //        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        //        {
        //            token = authHeader.Substring("Bearer ".Length).Trim();
        //        }

        //        var decodedToken = _jwtService.DecodedToken(token);

        //        return Ok(new ApiResponse<Object> { statusCode = 200, Message = "",
        //            Data = new {
        //                dataToken = decodedToken,
        //                code = 200 
        //            } });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"❌ Lỗi: {ex.Message}");
        //    }
        //}





    }
}
