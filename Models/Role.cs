using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebApplication3.Models.Entities;

namespace WebApplication3.Models
{
    [Table("tblRole")]
    public class Role : IEntity
    {
        [Key]
        [Column("RoleID")]
        public int RoleID { get; set; }

        [Column("RoleName")]
        public string RoleName { get; set; }  = string.Empty ;  

        [Column("isActive")]
        public bool IsActive { get; set; } = true;

        // Sử dụng JsonIgnore để tránh circular reference khi serialize
        [JsonIgnore]
        public ICollection<User> Users { get; set; }  = new List<User>();   //một tập hợp (collection) các phần tử kiểu User.

        [JsonIgnore]
        public ICollection<Driver> Drivers { get; set; } = new List<Driver>(); //một tập hợp (collection) các phần tử kiểu Driver.
    }



}
