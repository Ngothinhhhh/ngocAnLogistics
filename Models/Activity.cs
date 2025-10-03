using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication3.Models.Entities;

namespace WebApplication3.Models
{
    [Table("tblActivity")]
    public class Activity : IEntity
    {
        [Key]
        [Column("ActivityID")]
        public int ActivityID { get; set; }

        [Column("OrderID")]
        public int OrderID { get; set; }
        [ForeignKey("OrderID")]
        public Order Order { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }

        [Column("FieldName")]
        [StringLength(100)]
        public string FieldName { get; set; }

        [Column("OldValue")]
        public string OldValue { get; set; }

        [Column("NewValue")]
        public string NewValue { get; set; }

        [Column("ActivityDetail")]
        public string? ActivityDetail { get; set; }

        [Required]
        [Column("ActivityDate")]
        public DateTime ActivityDate { get; set; }

        [Column("IP")]
        [StringLength(50)]
        public string? IP { get; set; }
    }
}
