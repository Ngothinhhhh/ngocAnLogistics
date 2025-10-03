using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApplication3.DTOs.UserDTO
{
    public class NewUserDTO
    {

        public string UserName {  get; set; }
        public string Password {  get; set; }
        public string FullName { get; set; }
        public int RoleID { get; set; }

    }
}
