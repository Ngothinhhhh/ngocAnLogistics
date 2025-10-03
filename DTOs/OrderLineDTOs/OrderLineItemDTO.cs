using WebApplication3.DTOs.ItemDTOs;
using WebApplication3.Models;

namespace WebApplication3.DTOs.OrderLineDTOs
{
    public class OrderLineItemDTO : OrderLineDTO
    {
        public CreateItemDTO OrderLineItem { get; set; } // ✅ Property, không init

        public OrderLineItemDTO(OrderLineDTO baseDto)
        {
            OrderLineId = baseDto.OrderLineId;
            OrderId = baseDto.OrderId;
            ItemID = baseDto.ItemID;
            ItemDescription = baseDto.ItemDescription;
            ItemCost = baseDto.ItemCost;
            hasInvoice = baseDto.hasInvoice;
            InvoiceName = baseDto.InvoiceName;
            InvoiceNo = baseDto.InvoiceNo;
            isActive = baseDto.isActive;
        }

        public OrderLineItemDTO() { }
    }
}
