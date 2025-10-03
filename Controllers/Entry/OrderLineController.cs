using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mysqlx.Crud;
using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.DTOs.OrderLineDTOs;
using WebApplication3.DTOs.UserDTO;
using WebApplication3.Middlewares;
using WebApplication3.Models;

namespace WebApplication3.Controllers.Entry
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderLineController : ControllerBase
    {
        private readonly FleetDB _context;
        public OrderLineController(FleetDB context)
        { 
            _context = context; 
        }


        [AuthMiddlewareEntry]
        [HttpPost("createOrderLine")]
        public async Task<IActionResult> CreateOrderLine([FromBody] OrderLine listOrderLineDTO)
        //public async Task<IActionResult> CreateOrderLine([FromBody] ListOrderLineDTO listOrderLineDTO)
        {
            try
            {
                //var userData = HttpContext.Items["User"] as Dictionary<string , string>;
                //var userID = userData["UserID"];
                //var listOrderline = listOrderLineDTO.orderLineDTO;

                //int totalRecordItems = await _context.Items.CountAsync();

                //var list  = new Dictionary<string ,OrderLine>();

                //for (int i = 0; i < listOrderline.Count ; i++)
                //{
                //    OrderLine orderlineDTO = new OrderLine()
                //    {
                //        OrderId = listOrderline[i].OrderId,
                //        ItemId = listOrderline[i].ItemID,
                //        ItemDescription = listOrderline[i].ItemDescription,
                //        ItemCost = listOrderline[i].ItemCost,
                //        hasInvoice = listOrderline[i].hasInvoice,
                //        InvoiceName = listOrderline[i].InvoiceName,
                //        InvoiceNo = listOrderline[i].InvoiceNo,
                //        IsActive = listOrderline[i].isActive,
                //    };
                //    list.TryAdd(listOrderline[i].ItemID.ToString(), orderlineDTO  );
                //}


                //for (int i = 0; i < totalRecordItems ; i++)
                //{
                //     int key = i + 1;
                //    if (!list.ContainsKey(key.ToString()))
                //    {
                //        OrderLine orderlineDTO = new OrderLine()
                //        {
                //            OrderId = listOrderLineDTO.OrderID,
                //            ItemId = i + 1,
                //            ItemDescription = "",
                //            ItemCost = 0,
                //            hasInvoice = false,
                //            InvoiceName = "",
                //            InvoiceNo = "",
                //            IsActive = false,
                //        };
                //        bool check = list.TryAdd(listOrderline[i].ItemID.ToString(), orderlineDTO);
                //        if (!check)
                //        {
                //            return Ok(new ApiResponse<string> { statusCode = 400, Message = "Có key trùng trong ItemId", Data = null });

                //        }
                //    }
                //}

                //_context.OrderLines.AddRange(list.Values);
                //await _context.SaveChangesAsync();  

                //return Ok(new ApiResponse<Object>
                //{
                //    statusCode = 200,
                //    Message = $"✅ Tạo OrderLine thành công.",
                //    Data = list
                //});
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"✅ Tạo OrderLine thành công.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }

    }
}
