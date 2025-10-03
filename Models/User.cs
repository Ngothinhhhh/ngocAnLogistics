using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text.Json.Serialization;
using WebApplication3.Models.Entities;

namespace WebApplication3.Models
{
    [Table("tblUser")]
    public class User : IEntity
    {
        [Key]
        [Column("UserID")]
        public int UserID { get; set; }

        [Column("UserName")]
        public string UserName { get; set; } = string.Empty;

        [Column("Password")]
        public string Password { get; set; } = string.Empty;

        [Column("FullName")]
        public string FullName { get; set; } = string.Empty;

        [Column("RoleID")]
        public int RoleID { get; set; }

        [ForeignKey("RoleID")]
        public Role Role { get; set; }

        [Column("isActive")]
        public bool IsActive { get; set; } = true;


        public Customer Customer { get; set; }
        public Driver Driver { get; set; }


        // Ignore navigation collection để tránh circular reference
        [JsonIgnore] // Không serialize password vì lý do bảo mật
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        //[JsonIgnore]
        public ICollection<Image> Images { get; set; } = new List<Image>();
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();

    }

}
