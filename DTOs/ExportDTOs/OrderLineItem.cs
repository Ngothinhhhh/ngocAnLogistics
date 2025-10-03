using WebApplication3.Models;

namespace WebApplication3.DTOs.ExportDTOs
{
    public class OrderLineItem : OrderLine
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public int FixedPrice { get; set; }
        public OrderLineItem() { }  
    }
}
