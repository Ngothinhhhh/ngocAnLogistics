namespace WebApplication3.DTOs.OrderLineDTOs
{


    public class OrderLineDTO
    {
        public int OrderLineId { get; set; }    
        public int OrderId { get; set; }
        public int ItemID { get; set; }
        public string? ItemDescription { get; set; }
        public int? ItemCost { get; set; }
        public bool hasInvoice { get; set; } 
        public string? InvoiceName { get; set; }
        public string? InvoiceNo { get; set; }
        public bool isActive { get; set; }
    }
}
