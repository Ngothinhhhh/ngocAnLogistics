using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication3.Models.Entities;

namespace WebApplication3.Models
{
    [Table("tblimages")]
    public class Image : IEntity
    {
        [Key]
        [Column("ImageID")]
        public int ImageID { get; set; }

        [Required]
        [Column("FileName")]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        [Column("URL")]
        [StringLength(500)]
        public string URL { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }

        [Column("Descrip")]
        [StringLength(500)]
        public string? Descrip { get; set; }

        [Required]
        [Column("Created")]
        public DateTime Created { get; set; } 

        [Column("isActive")]
        public bool isActive { get; set; } = true;

        [Column("OrderID")]
        public int OrderID { get; set; }
        [ForeignKey("OrderID")]
        public Order Order { get; set; }

    }
}
