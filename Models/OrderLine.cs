using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication3.Models.Entities;

namespace WebApplication3.Models
{
    [Table("tblOrderLine")]
    public class OrderLine : IEntity
    {
        [Key]
        [Column("OrderLineID")]
        public int OrderLineId { get; set; }

        // Foreign Keys (NOT NULL)
        [Column("OrderID")]
        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [Column("ItemId")]
        [Required]
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }

        // Nullable fields
        [Column("ItemDescription")]
        [MaxLength(255)]
        public string? ItemDescription { get; set; }

        [Column("ItemCost")]
        public long? ItemCost { get; set; }

        // Bool with default false
        [Column("hasInvoice")]
        public bool HasInvoice { get; set; } = false;

        [Column("isActive")] // ví dụ bool khác
        public bool IsActive { get; set; } = false;

        [Column("InvoiceName")]
        [MaxLength(255)]
        public string? InvoiceName { get; set; }

        [Column("InvoiceNo")]
        [MaxLength(255)]
        public string? InvoiceNo { get; set; }


    }
}
