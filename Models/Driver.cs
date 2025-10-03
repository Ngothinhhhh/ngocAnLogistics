using Mysqlx.Crud;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebApplication3.Models.Entities;

namespace WebApplication3.Models
{
    [Table("tblDriver")]
    public class Driver : IEntity
    {
        [Key]
        [Column("DriverID")]
        public int DriverID { get; set; }

        [Column("DriverName")]
        public string DriverName { get; set; } = string.Empty;

        [Column("RoleID")]
        public int? RoleID { get; set; }
        [ForeignKey("RoleID")]
        public Role Role { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }


        [Column("Address")]
        public string Address { get; set; } = string.Empty;

        [Column("Phone")]
        public string Phone { get; set; } = string.Empty;

        [Column("LicenseNo")]
        public string LicenseNo { get; set; } = string.Empty;

        [Column("ExpireDate")]
        public string ExpireDate { get; set; }

        [Column("isActive")]
        public bool IsActive { get; set; } = true;

        public enum StatusDriver
        {
            Available,  // Đang rảnh - có thể nhận đơn
            Busy        // Đang bận - đang thực hiện đơn
        }
        [Column("Status")]
        public StatusDriver Status { get; set; } = StatusDriver.Available;


        // Ignore navigation collection để tránh circular reference
        [JsonIgnore]
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }


}
