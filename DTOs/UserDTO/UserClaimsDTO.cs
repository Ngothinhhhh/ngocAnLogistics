using WebApplication3.Models;

namespace WebApplication3.DTOs.UserDTO
{
    public class UserClaimsDTO
    {
        public int UserID { get; set; } 
        public string UserName  { get; set; }
        public string FullName { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }


    }
}
