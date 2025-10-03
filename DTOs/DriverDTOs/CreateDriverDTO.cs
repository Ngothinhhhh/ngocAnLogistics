using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApplication3.DTOs.DriverDTOs
{
    public class CreateDriverDTO
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string DriverName {  get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string LicenseNo { get; set; }
        public string ExpireDate { get; set; }
    }
}
