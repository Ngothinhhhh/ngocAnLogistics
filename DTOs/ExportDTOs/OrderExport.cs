using WebApplication3.Models;

namespace WebApplication3.DTOs.ExportDTOs
{
    public class OrderExport : Order
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public int FixedPrice { get; set; }

        public ICollection<OrderLineItem> OrderLineList { get; set; }  

        public OrderExport()
        {
        }
    }
}
