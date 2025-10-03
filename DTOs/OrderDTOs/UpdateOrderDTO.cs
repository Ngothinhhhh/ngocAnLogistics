using WebApplication3.DTOs.OrderLineDTOs;
using WebApplication3.Models;

namespace WebApplication3.DTOs.OrderDTOs
{
    public class UpdateOrderDTO
    {
        public string OrderDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? DriverId { get; set; }
        public string? DriverName { get; set; }
        public int? TruckId { get; set; }
        public string? TruckNo { get; set; }
        public int? RmoocId { get; set; }
        public string? RmoocNo { get; set; }
        public string ContainerNo { get; set; }
        public string ContainerType { get; set; }
        public string BillBookingNo { get; set; }
        public int FromLocationID { get; set; }
        public int FromWhereID { get; set; }
        public int ToLocationID { get; set; }
        public string FromLocationName { get; set; }
        public string FromWhereName { get; set; }
        public string ToLocationName { get; set; }

        public Order.OrderStatus Status { get; set; }
        public DateTime RowVersion { get; set; }
        public DateTime CreatedDate { get; set; }

        //public List<OrderLineTest> OrderLineList1 { get; set; } = new List<OrderLineTest>();
        //public List<OrderLineDTO> OrderLineList1 { get; set; } = new List<OrderLineDTO>();
        public List<OrderLineItemDTO> OrderLineList1 { get; set; } = new List<OrderLineItemDTO>();
        public List<OrderLineDTO> OrderLineList { get; set; } = new List<OrderLineDTO>();
        public List<Image> OrderImageList { get; set; } = new List<Image>();
    }
}
