using Mysqlx.Crud;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebApplication3.Models.Entities;

namespace WebApplication3.Models
{
    [Table("tblTruck")]
    public class Truck : IEntity
    {
        [Key]
        [Column("TruckID")]
        public int TruckID { get; set; }

        [Column("TruckNo")]
        public string TruckNo { get; set; } = string.Empty;

        [Column("TruckType")]
        public int TruckType { get; set; } = 1; // 1 = Container ; 2 = Rmooc 

        [Column("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public ICollection<Order> OrdersAsTruck { get; set; } = new List<Order>();
        [JsonIgnore]
        public ICollection<Order> OrdersAsRmooc { get; set; } = new List<Order>();
    }

}
