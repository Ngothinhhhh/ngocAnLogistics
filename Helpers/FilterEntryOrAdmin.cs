using Microsoft.AspNetCore.Mvc;
using WebApplication3.DTOs.UserDTO;

namespace WebApplication3.Helpers
{
    public static class FilterEntryOrAdmin
    {
        public static IActionResult CheckEntryOrAdmin(this HttpContext context)
        {
            var user = context.Items["User"] as UserClaimsDTO;

            if (user == null )
            {
                return new UnauthorizedObjectResult("Bạn không có quyền này.");
            }
            if ((user.RoleName == "Admin" && user.RoleID == 1) || ( user.RoleName == "Entry" && user.RoleID == 5))
            {
                return null;
            }
            else
            {
                return new UnauthorizedObjectResult("Bạn không có quyền này.");
            }
            return null;
        }
    }
}
