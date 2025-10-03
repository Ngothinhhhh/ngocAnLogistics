using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.DTOs.ActivityDTOs;
using WebApplication3.Helpers;
using WebApplication3.Middlewares;
using WebApplication3.Models;
using WebApplication3.Services.Services.Activitys;

namespace WebApplication3.Controllers.RoleDriver
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverRoleController : ControllerBase
    {
        private readonly FleetDB _context;
        private readonly IActivityService _activityService;
        public DriverRoleController(FleetDB context, IActivityService activityService)
        {
            _context = context;
            _activityService = activityService;
        }



        [JwtAuthDriver]
        [HttpGet("listOrderForDriver")]
        public async Task<IActionResult> ListOrder(
            [FromQuery] string order = "desc", // ✅ Default value
            [FromQuery] string sortBy = "id", // ✅ Default value  
            [FromQuery] int pageSize = 13,    // ✅ Default value
            [FromQuery] int pageNumber = 1    // ✅ Default value
        )
        {
            //  Get By ID Driver
            try
            {
                var dataDriver = HttpContext.Items["driver"] as Dictionary<string, string>;
                int.TryParse(dataDriver["UserID"], out var userID) ;
                var findDriverID = await _context.Drivers.Where( d=> d.UserID == userID).Select(d => d.DriverID ).FirstOrDefaultAsync();
                var driverID = findDriverID;

                var query = _context.Orders
                    .Where(o => o.IsDelete == false && o.DriverId == driverID
                );

                switch (sortBy.ToLower())
                {
                    case "orderdate":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(o => o.OrderDate)
                            : query.OrderBy(o => o.OrderDate);
                        break;
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(o => o.OrderID)
                            : query.OrderBy(o => o.OrderID);
                        break;
                }

                var size = Math.Max(1 ,pageSize);
                var page = Math.Max(1 ,pageNumber);


                var listOrder = await query
                    .Select(o => new { o.OrderID, o.CustomerName, o.Status ,o.OrderDate,o.TotalCost  })
                    .Skip((page - 1) * size) 
                    .Take(size)
                    .ToListAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Lấy danh sách đơn hàng thành công",
                    Data = listOrder
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthDriver]
        [HttpGet("detailOrderForDriver")]
        public async Task<IActionResult> DetailOrderForDriver(
            [FromQuery] int orderID
        )
        {
            //  Get By ID Driver
            try
            {
                var dataDriver = HttpContext.Items["driver"] as Dictionary<string, string>;
                int.TryParse(dataDriver["UserID"], out var userID);
                var findDriverID = await _context.Drivers.Where(d => d.UserID == userID).Select(d => d.DriverID).FirstOrDefaultAsync();
                var driverID = findDriverID;

                var query = _context.Orders
                    .Where(o => o.IsDelete == false && o.OrderID == orderID && o.DriverId == driverID
                );

                var detailOrder = await query
                    .Select(o => new {
                        o.OrderID,
                        o.CustomerName ,
                        o.FromLocationName, o.FromWhereName , o.ToLocationName ,
                        o.ContainerNo , o.TruckNo, o.RmoocNo,
                        o.Status, o.OrderDate
                    }) // status trạng thái nào thì để band màu đó.
                    .FirstOrDefaultAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Lấy danh sách đơn hàng thành công",
                    Data = detailOrder
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthDriver]
        [HttpPut("updateStatusOrderForDriver")]
        public async Task<IActionResult> UpdateStatusOrderForDriver(
            [FromQuery] int orderID,
            [FromQuery] string statusString
        )
        {
            try
            {
                var dataDriver = HttpContext.Items["driver"] as Dictionary<string, string>;
                int.TryParse(dataDriver["UserID"], out var userID);
                var findDriverID = await _context.Drivers.Where(d => d.UserID == userID).Select(d => d.DriverID).FirstOrDefaultAsync();
                var driverID = findDriverID;

                var detailOrder = await _context.Orders
                    .Where(o => o.IsDelete == false && o.OrderID == orderID && o.DriverId == driverID)
                    .FirstOrDefaultAsync();

                if (!Enum.TryParse<Order.OrderStatus>(statusString, true, out var status))
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"Trạng thái không hợp lệ: {statusString}", Data = null });
                }
                if (detailOrder == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Đơn hàngg không tìm thấy", Data = null });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var activityDTO = new ActivityForCreation()
                {
                    OrderID = orderID,
                    FieldName = "Status",
                    UserID = userID,
                    OldValue = detailOrder.Status.ToString(),
                    NewValue = statusString,
                    IP = ipAddress,
                    ActivityDetail = $"Thay đổi trạng thái từ '{detailOrder.Status.ToString()}' thành {status.ToString()}"
                };
                await _activityService.CreateActivity(activityDTO);


                detailOrder.Status = status;

                _context.Orders.Update(detailOrder);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Cập nhật trạng thái đơn hàng thành công",
                    Data = detailOrder
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


    }
}
