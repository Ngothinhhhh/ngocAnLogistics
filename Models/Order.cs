using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebApplication3.Models.Entities;

namespace WebApplication3.Models
{
    [Table("tblOrder")]
    public class Order : IEntity
    {
        [Key]
        [Column("OrderID")]
        public int OrderID { get; set; }

        [Column("OrderDate")]
        public DateTime OrderDate { get; set; }

        [Column("UserId")]
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Column("CustomerId")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column("TotalCost")]
        public int TotalCost { get; set; } = 0;

        public string CustomerName { get; set; }

        [Column("DriverId")]
        public int? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public Driver Driver { get; set; }
        [Column("DriverName")]
        public string? DriverName { get; set; } = string.Empty; 


        [Column("TruckId")]
        public int? TruckId { get; set; }
        [ForeignKey("TruckId")]
        public Truck Truck { get; set; }

        [Column("RmoocId")]
        public int? RmoocId { get; set; }
        [ForeignKey("RmoocId")]
        public Truck Rmooc { get; set; }   // Navigation

        [Column("TruckNo")]
        public string? TruckNo { get; set; } = string.Empty;

        [Column("RmoocNo")]
        public string? RmoocNo { get; set; } = string.Empty;

        [Column("ContainerNo")]
        public string ContainerNo { get; set; } = string.Empty;

        [Column("ContainerType")]
        public string ContainerType { get; set; } = string.Empty;

        [Column("BillBookingNo")]
        public string BillBookingNo { get; set; } = string.Empty;

        [Column("FromLocationId")]
        public long FromLocationId { get; set; }

        [ForeignKey("FromLocationId")]
        public Location FromLocation { get; set; }
        [Column("FromLocationName")]
        public string FromLocationName { get; set; }

        [Column("FromWhereId")]
        public long FromWhereId { get; set; }

        [ForeignKey("FromWhereId")]
        public Location FromWhere { get; set; }
        [Column("FromWhereName")]
        public string FromWhereName { get; set; }


        [Column("ToLocationId")]
        public long ToLocationId { get; set; }

        [ForeignKey("ToLocationId")]
        public Location ToLocation { get; set; }
        [Column("ToLocationName")]
        public string ToLocationName { get; set; }


        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [Column("UpdatedDate")]
        public DateTime UpdatedDate { get; set; }



        public enum OrderStatus
        {
            Pending,        // đag chờ được xử lí 
            InProgress,      // Đang xu ly
            PickedUp,       // Đã lấy hàng
            InTransit,      // Đang vận chuyển
            Delivered,      // Đã giao
            AwaitingApproval, // Chờ duyệt & tính toán chi phí
            Completed,      // Hoàn thành
            Cancelled,      // Đã hủy
            FailedDelivery  // Giao thất bại
        }
        [Column("Status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;


        [Column("isDelete")]
        public bool IsDelete { get; set; } = false;

        // Concurrency token (SQL Server sẽ tự tăng mỗi khi record bị update)
        [ConcurrencyCheck]
        public DateTime RowVersion { get; set; }

        //[JsonIgnore]
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
        //[JsonIgnore]
        public ICollection<Image> Images { get; set; } = new List<Image>();
        public ICollection<Activity> Activitys { get; set; } = new List<Activity>();

    }
    
}
