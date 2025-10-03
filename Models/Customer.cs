using Mysqlx.Crud;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebApplication3.Models.Entities;

namespace WebApplication3.Models
{
    [Table("tblCustomer")]
    public class Customer : IEntity
    {
        [Key]
        [Column("CustomerID")]
        public int CustomerID { get; set; }

        //
        [Column("UserID")]
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }

        //

        [Column("CustomerCode")]
        public string CustomerCode { get; set; } = string.Empty;

        [Column("CustomerName")]
        public string CustomerName { get; set; } = string.Empty;

        [Column("CustomerAddr")]
        public string CustomerAddr { get; set; } = string.Empty;

        [Column("DisplayOrder")]
        public int DisplayOrder { get; set; }

        [Column("isActive")]
        public bool IsActive { get; set; } = true;

        [Column("TaxNo")]
        public string TaxNo { get; set; } = string.Empty;

        [Column("ContactName")]
        public string ContactName { get; set; } = string.Empty;

        [Column("ContactPhone")]
        public string ContactPhone { get; set; } = string.Empty;


        // Ignore navigation collection để tránh circular reference
        [JsonIgnore]
        public ICollection<Order> Orders { get; set; } = new List<Order>();
                                                                                //ICollection là 1 interface của Collection có thể thêm xóa sửa 
                                                                                // 1 Collection có thể thêm xóa sửa kiểu Order
}

}
