using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebApplication3.Models.Entities;

namespace WebApplication3.Models
{
    [Table("tblItems")]
    public class Item : IEntity
    {
        [Key]
        [Column("ItemID")]
        public int ItemID { get; set; }

        [Column("ItemName")]
        public string ItemName { get; set; } = string.Empty;

        [Column("FixedPrice")]
        public long FixedPrice { get; set; }

        [Column("DisplayOrder")]
        public int DisplayOrder { get; set; }

        [Column("isActive")]
        public bool IsActive { get; set; } = true ;

        // Ignore navigation collection để tránh circular reference
        //[JsonIgnore]
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }

}
