using Microsoft.AspNetCore.Mvc;
using WebApplication3.DTOs;
using WebApplication3.DTOs.UserDTO;

namespace WebApplication3.Helpers
{
    public static class FilterAdmin
    {
        public static IActionResult CheckAdmin(this HttpContext context)
        {
            var user = context.Items["User"] as UserClaimsDTO;
            if (user == null || user.RoleName != "Admin" || user.RoleID != 1)
            {
                return new UnauthorizedObjectResult("Bạn không có quyền này.");
            }
            return null;
        }
    } 
}
