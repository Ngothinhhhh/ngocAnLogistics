using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.DTOs.UserDTO;
using WebApplication3.Helpers;
using WebApplication3.Middlewares;
using WebApplication3.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;            //*********************

namespace WebApplication3.Controllers.Admin
{
    [Route("api/admin/[Controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {

        private readonly FleetDB _context;
        public RoleController(FleetDB context) 
        {
            _context = context;
        }


        [JwtAuthMiddleware]
        [HttpPost("createRole")] 
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDTO roleDTO)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                if (string.IsNullOrEmpty(roleDTO.RoleName))
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ RoleName không được để trống.", Data = null });
                }
                var exitRoleName = await _context.Roles.AnyAsync(r => r.RoleName == roleDTO.RoleName);
                if (exitRoleName)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Role '{roleDTO.RoleName}' đã tồn tại.", Data = null });
                }
                var role = new Role() { RoleName = roleDTO.RoleName };
                _context.Roles.Add(role);           // Đưa vào bộ nhớ EF (state: Added)
                await _context.SaveChangesAsync();  // INSERT INTO Roles (RoleName) VALUES ('Admin')
                
                return Ok(new ApiResponse<System.Object>
                {
                    statusCode = 200,
                    Message = $"✅ Tạo Role {role.RoleName} thành công.",
                    Data = role
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }

        [JwtAuthMiddleware]
        [HttpDelete("deleteRole/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == id) ;
                if (role == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"❌ Role ID {id} không tồn tại.", Data = null });
                }
                role.IsActive = false; 
                _context.Roles.Update(role);
                await _context.SaveChangesAsync();
                return Ok(new ApiResponse<System.Object>
                {
                    statusCode = 200,
                    Message = $"Xóa thành công ( RoleName : {role.RoleName} -- ID Role : {role.RoleID} )",
                    Data = role
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }

        [JwtAuthMiddleware]
        [HttpPut("updateRole/{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDTO roleDTO)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }

                if (string.IsNullOrEmpty(roleDTO.RoleName))
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "RoleName không được để trống.", Data = null });
                }

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == id);
                if (role == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"❌ Role ID {id} không tồn tại.", Data = null });
                }

                var existsRoleName = await _context.Roles
                    .AnyAsync(r => r.RoleName.ToLower() == roleDTO.RoleName.ToLower() && r.RoleID != id);
                if (existsRoleName)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Role '{roleDTO.RoleName}' đã tồn tại.", Data = null });
                }
                role.RoleName = roleDTO.RoleName;
                role.IsActive = roleDTO.isActive;
                _context.Roles.Update(role);
                await _context.SaveChangesAsync();
                
                return Ok(new ApiResponse<System.Object>
                {
                    statusCode = 200,
                    Message = $"✅ Đã cập nhật Role ID {id} thành công.",
                    Data = role
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpGet("getListRole")] 
        public async Task<IActionResult> GetListRole()
        {
            var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
            if (checkAdmin != null)
            {
                return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
            }
            var query = _context.Roles.Where( r => r.IsActive == true);
            var ListRole = await query.ToListAsync();
            return Ok(ListRole);
        }

    }
}
