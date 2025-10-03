using Mysqlx.Crud;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebApplication3.Models.Entities;

namespace WebApplication3.Models
{
    [Table("tblLocation")]
    public class Location : IEntity
    {
        [Key]
        [Column("LocationID")]
        public long LocationId { get; set; }

        [Column("LocationName")]
        [MaxLength(255)]
        public string LocationName { get; set; }

        [Column("isActive")]
        public bool IsActive { get; set; } = true;


        [JsonIgnore]
        public virtual ICollection<Order> OrdersFromLocation { get; set; } = new List<Order>();
        [JsonIgnore]
        public virtual ICollection<Order> OrdersFromWhere { get; set; } = new List<Order>();
        [JsonIgnore]
        public virtual ICollection<Order> OrdersToLocation { get; set; } = new List<Order>();

    }

}
