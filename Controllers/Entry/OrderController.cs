using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.DTOs.ActivityDTOs;
using WebApplication3.DTOs.ImageDTOs;
using WebApplication3.DTOs.ItemDTOs;
using WebApplication3.DTOs.OrderDTOs;
using WebApplication3.DTOs.OrderLineDTOs;
using WebApplication3.DTOs.RequestDTOs;
using WebApplication3.DTOs.UserDTO;
using WebApplication3.Helpers;
using WebApplication3.Middlewares;
using WebApplication3.Models;
using WebApplication3.Services.Services.Activitys;
using WebApplication3.Services.Services.Orders;
using WebApplication3.Services.Services.Photos;


namespace WebApplication3.Controllers.Entry
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly FleetDB _context;
        private readonly IImageService _imageService;
        private readonly IActivityService _activityService;
        private readonly IOrderService _orderService;   
        public OrderController(FleetDB context, IImageService imageService, IActivityService activityService, IOrderService orderService)
        {
            _context = context;
            _imageService = imageService;
            _activityService = activityService;
            _orderService = orderService;
        }


        [JwtAuthMiddleware]
        [HttpPost("createOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO orderDTO)
        {
            var transaction = await _context.Database.BeginTransactionAsync();    
            try
            {
                var userData = HttpContext.Items["User"] as UserClaimsDTO;
                int userID = userData.UserID;

                var validate = ValidateString.StringValidator( new Dictionary<string, string>
                {
                    {"Ngày đặt hàng" , orderDTO.OrderDate },
                    {"ID khách hàng" , orderDTO.CustomerId.ToString() },
                    {"Name khách hàng" , orderDTO.CustomerName },
                    //{"ID tài xế" , orderDTO.DriverId.ToString() },
                    //{"Tên tài xế" , orderDTO.DriverName },
                    //{"ID Container" , orderDTO.TruckId.ToString() },
                    //{"Biển số Container" , orderDTO.TruckNo },
                    //{"ID Rmooc" , orderDTO.RmoocId.ToString() },
                    //{"Biển số Rmooc" , orderDTO.RmoocNo },
                    {"Số Container" , orderDTO.ContainerNo },
                    {"Loại Container" , orderDTO.ContainerType },
                    {"Số Billing Booking" , orderDTO.BillBookingNo },
                    {"ID Cảng nhận Cont" , orderDTO.FromLocationID.ToString() },
                    {"ID Kho nhận/giao hàng" , orderDTO.FromWhereID.ToString() },
                    {"ID Cảng hạ/trả Cont" , orderDTO.ToLocationID.ToString() },
                    {"Cảng nhận Cont" , orderDTO.FromLocationName },
                    {"Kho nhận/giao hàng" , orderDTO.FromWhereName },
                    {"Cảng hạ/trả Cont" , orderDTO.ToLocationName },
                });
                if (validate != null)
                {
                    return Ok(validate);
                }
                if (!DateTime.TryParse(orderDTO.OrderDate , out var OrderDate))
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không đúng định dạng ngày tháng", Data = null });
                }
                if (OrderDate.Date < DateTime.Now.Date)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không thể đặt hàng ở quá khứ", Data = null });
                }
                var customerExistsTask = await _context.Customers.AnyAsync(c => c.IsActive && c.CustomerID == orderDTO.CustomerId);

                var fromPortExistsTask = await _context.Locations.AnyAsync(l => l.IsActive && l.LocationId == orderDTO.FromLocationID);

                var fromWarehouseExistsTask = await _context.Locations.AnyAsync(l => l.IsActive && l.LocationId == orderDTO.FromWhereID);

                var toPortExistsTask = await  _context.Locations.AnyAsync(l => l.IsActive && l.LocationId == orderDTO.ToLocationID);
                if (!customerExistsTask)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "CustomerID không tồn tại", Data = null });
                if (!fromPortExistsTask)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Kho nhận Container không tồn tại", Data = null });
                if (!fromWarehouseExistsTask)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Kho nhận Container không tồn tại", Data = null });
                if (!toPortExistsTask)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Cảng hạ Container không tồn tại", Data = null });

                Models.Order newOrder = new Models.Order()
                {
                    OrderDate = OrderDate,
                    UserId = userID,   
                    CustomerId = orderDTO.CustomerId,   
                    CustomerName = orderDTO.CustomerName,   
                    ContainerNo = orderDTO.ContainerNo,
                    ContainerType = orderDTO.ContainerType,
                    BillBookingNo = orderDTO.BillBookingNo,
                    FromLocationId = orderDTO.FromLocationID,
                    FromWhereId = orderDTO.FromWhereID,
                    ToLocationId =  orderDTO.ToLocationID,
                    FromLocationName = orderDTO.FromLocationName,
                    FromWhereName = orderDTO.FromWhereName,
                    ToLocationName = orderDTO.ToLocationName,
                    CreatedDate = DateTime.Now,
                };   
                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                var list = new List<OrderLine>();

                int totalCost = 0;

                foreach (var item in orderDTO.OrderLineList)
                {
                    
                    OrderLine orderlineDTO = new OrderLine()
                    {
                        OrderId = newOrder.OrderID,
                        ItemId = item.ItemID ,
                        ItemDescription = item.ItemDescription,
                        ItemCost = item.ItemCost,
                        HasInvoice = item.hasInvoice,
                        InvoiceName = item.InvoiceName,
                        InvoiceNo = item.InvoiceNo,
                        IsActive = item.isActive
                    };
                    totalCost = (int)(totalCost + item.ItemCost);
                    list.Add(orderlineDTO);
                }
                newOrder.TotalCost = totalCost; 
                _context.OrderLines.AddRange(list);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"✅ Tạo đơn hàng thành công - OrderID : '{newOrder.OrderID}' ",
                    Data = list
                });
              
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return Ok(new ApiResponse<string> { statusCode = 400, Message = "Order đã bị người khác cập nhật. Vui lòng load lại trước khi sửa.", Data = null });
            }
            catch (DbUpdateException ex) // More general database update exception
            {
                await transaction.RollbackAsync();
                return Ok(new ApiResponse<string>
                {
                    statusCode = 400,
                    Message = "Lỗi khi tạo đơn hàng. Vui lòng thử lại.",
                    Data = null
                });
            }
        }


        [JwtAuthMiddleware]
        [HttpGet("listOrder")]
        public async Task<IActionResult> ListOrder(
            [FromQuery] string fromDateStr = ""  , // ✅ Default value
            [FromQuery] string toDateStr = "", // ✅ Default value
            [FromQuery] string order = "asc", // ✅ Default value
            [FromQuery] string sortBy = "id", // ✅ Default value  
            [FromQuery] int pageSize = 30,    // ✅ Default value
            [FromQuery] int pageNumber = 1,    // ✅ Default value
            [FromQuery] string searchKey = "", // ✅ Default value  
            [FromQuery] string status = "" // ✅ Default value  
        )
        {
            try
            {
                DateTime? fromDate = null;
                DateTime? toDate = null;
                // Kiểm tra và chuyển đổi chuỗi ngày từ
                if (!string.IsNullOrEmpty(fromDateStr) &&
                    DateTime.TryParse(fromDateStr, out var parsedFromDate))
                {
                    fromDate = parsedFromDate;
                }

                if (!string.IsNullOrEmpty(toDateStr) &&
                    DateTime.TryParse(toDateStr, out var parsedToDate))
                {
                    // lấy đến cuối ngày
                    toDate = parsedToDate.Date.AddDays(1).AddSeconds(-1);
                }

                // Validation date range
                if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
                {
                    return Ok(new ApiResponse<string>
                    {
                        statusCode = 400,
                        Message = "Ngày bắt đầu không thể lớn hơn ngày kết thúc",
                        Data = null
                    });
                }

                Order.OrderStatus? statusEnum = null;

                if (!string.IsNullOrEmpty(status) && Enum.TryParse<Order.OrderStatus>(status, true, out Order.OrderStatus parsedStatus))
                {
                    statusEnum = parsedStatus;
                }

                var query = _context.Orders.Where(o => o.IsDelete == false);

                if (!string.IsNullOrEmpty(searchKey))
                {
                    query = query.Where( o => 
                                o.OrderID.ToString().Contains(searchKey)
                             || o.ContainerNo.Contains(searchKey)
                             || o.BillBookingNo.Contains(searchKey)
                             || o.CustomerName.Contains(searchKey)  
                             || o.TruckId.ToString().Contains(searchKey)
                             || o.RmoocNo.Contains(searchKey)
                             || o.FromLocationName.Contains(searchKey)
                             || o.ToLocationName.Contains(searchKey)
                             || o.FromWhereName.Contains(searchKey) );
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate <= toDate.Value);
                }

                if (statusEnum.HasValue)
                {
                    query = query.Where(o => o.Status == statusEnum.Value);
                }


                switch (sortBy.ToLower())
                {
                    case "cost":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(o => o.TotalCost)
                            : query.OrderBy(o => o.TotalCost);
                        break;
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(t => t.OrderID)
                            : query.OrderBy(t => t.OrderID);
                        break;
                }
                
                var size = Math.Max(1, pageSize);
                var page = Math.Max(1, pageNumber);
                
                var listOrder = await query
                    .Skip((page - 1) * size)
                    .Take(size)
                    .Select(o => new Models.Order
                    {
                        OrderDate = o.OrderDate,
                        OrderID = o.OrderID,
                        UserId = o.UserId,
                        CustomerName = o.CustomerName,
                        DriverName = o.DriverName,
                        TruckNo = o.TruckNo,
                        RmoocNo = o.RmoocNo,
                        ContainerNo = o.ContainerNo,
                        BillBookingNo = o.BillBookingNo,    
                        FromLocationName = o.FromLocationName,
                        ToLocationName = o.ToLocationName,
                        FromWhereName = o.FromWhereName,
                        Status = o.Status,
                        TotalCost = o.TotalCost
                    })
                    .ToListAsync();
                                    
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Lấy danh sách đơn hàng thành công",
                    Data =  new { listOrder, fromDate, toDate }
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }
        //[JwtAuthMiddleware]
        //[HttpGet("listOrder")]
        //public async Task<IActionResult> ListOrder(
        //    [FromQuery] string fromDateStr = ""  , // ✅ Default value
        //    [FromQuery] string toDateStr = "", // ✅ Default value
        //    [FromQuery] string order = "desc", // ✅ Default value
        //    [FromQuery] string sortBy = "id", // ✅ Default value  
        //    [FromQuery] int pageSize = 30,    // ✅ Default value
        //    [FromQuery] int pageNumber = 1,    // ✅ Default value
        //    [FromQuery] string searchKey = "", // ✅ Default value  
        //    [FromQuery] string status = "", // ✅ Default value  
        //    [FromQuery] string cursor = "" // ✅ Default value   
        //)
        //{
        //    try
        //    {
        //        var adminOrEntry = FilterEntryOrAdmin.CheckEntryOrAdmin(HttpContext);
        //        if (adminOrEntry != null)
        //        {
        //            return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin hoặc Entry.", Data = null });
        //        }

        //        DateTime? fromDate = null;
        //        DateTime? toDate = null;
        //        // Kiểm tra và chuyển đổi chuỗi ngày từ
        //        if (!string.IsNullOrEmpty(fromDateStr) &&
        //            DateTime.TryParse(fromDateStr, out var parsedFromDate))
        //        {
        //            fromDate = parsedFromDate;
        //        }

        //        if (!string.IsNullOrEmpty(toDateStr) &&
        //            DateTime.TryParse(toDateStr, out var parsedToDate))
        //        {
        //            // lấy đến cuối ngày
        //            toDate = parsedToDate.Date.AddDays(1).AddSeconds(-1);
        //        }

        //        // Validation date range
        //        if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
        //        {
        //            return Ok(new ApiResponse<string>
        //            {
        //                statusCode = 400,
        //                Message = "Ngày bắt đầu không thể lớn hơn ngày kết thúc",
        //                Data = null
        //            });
        //        }

        //        Order.OrderStatus? statusEnum = null;

        //        if (!string.IsNullOrEmpty(status) && Enum.TryParse<Order.OrderStatus>(status, true, out Order.OrderStatus parsedStatus))
        //        {
        //            statusEnum = parsedStatus;
        //        }

        //        var query = _context.Orders.Where(o => o.IsDelete == false);

        //        if (!string.IsNullOrEmpty(searchKey))
        //        {
        //            query = query.Where( o => 
        //                        o.OrderID.ToString().Contains(searchKey)
        //                     || o.ContainerNo.Contains(searchKey)
        //                     || o.TruckId.ToString().Contains(searchKey)
        //                     || o.RmoocNo.Contains(searchKey)
        //                     || o.FromLocationName.Contains(searchKey)
        //                     || o.ToLocationName.Contains(searchKey)
        //                     || o.FromWhereName.Contains(searchKey) );
        //        }

        //        if (fromDate.HasValue)
        //        {
        //            query = query.Where(o => o.OrderDate >= fromDate.Value);
        //        }

        //        if (toDate.HasValue)
        //        {
        //            query = query.Where(o => o.OrderDate <= toDate.Value);
        //        }

        //        if (statusEnum.HasValue)
        //        {
        //            query = query.Where(o => o.Status == statusEnum.Value);
        //        }


        //        switch (sortBy.ToLower())
        //        {
        //            case "cost":
        //                query = order.ToLower() == "desc"
        //                    ? query.OrderByDescending(o => o.TotalCost)
        //                    : query.OrderBy(o => o.TotalCost);
        //                break;
        //            default:
        //                query = order.ToLower() == "desc"
        //                    ? query.OrderByDescending(t => t.OrderID)
        //                    : query.OrderBy(t => t.OrderID);
        //                break;
        //        }

        //        if (!string.IsNullOrEmpty(cursor))
        //        {
        //            switch (sortBy.ToLower())
        //            {
        //                case "cost":
        //                    if (int.TryParse(cursor, out int cursorCost))
        //                    {
        //                        if (order.ToLower() == "desc")
        //                        {
        //                            // DESC: lấy những record có cost < cursor (nhỏ hơn)
        //                            query = query.Where(o => o.TotalCost < cursorCost);
        //                        }
        //                        else
        //                        {
        //                            // ASC: lấy những record có cost > cursor (lớn hơn)
        //                            query = query.Where(o => o.TotalCost > cursorCost);
        //                        }
        //                    }
        //                    break;
        //                default:
        //                    if (int.TryParse(cursor, out int cursorID))
        //                    {
        //                        if (order.ToLower() == "desc")
        //                        {
        //                            // DESC: lấy những record có id < cursor (nhỏ hơn)
        //                            query = query.Where(o => o.OrderID < cursorID);
        //                        }
        //                        else
        //                        {
        //                            // ASC: lấy những record có id > cursor (lớn hơn)
        //                            query = query.Where( o => o.OrderID > cursorID);
        //                        }
        //                    }
        //                    break;
        //            }
        //        }

        //        var size = Math.Max(1, pageSize);
        //        var page = Math.Max(1, pageNumber);

        //        var listOrder = new List<Models.Order>();  

        //        if (string.IsNullOrEmpty(cursor))
        //        {
        //            listOrder = await query
        //            .Skip((page - 1) * size)
        //            .Take(size)
        //            .Select(o => new Models.Order
        //            {
        //                OrderDate = o.OrderDate,
        //                OrderID = o.OrderID,
        //                UserId = o.UserId,
        //                CustomerName = o.CustomerName,
        //                DriverName = o.DriverName,
        //                TruckNo = o.TruckNo,
        //                RmoocNo = o.RmoocNo,
        //                ContainerNo = o.ContainerNo,
        //                FromLocationName = o.FromLocationName,
        //                ToLocationName = o.ToLocationName,
        //                FromWhereName = o.FromWhereName,
        //                Status = o.Status,
        //                TotalCost = o.TotalCost
        //            })
        //            .ToListAsync();
        //        }
        //        else
        //        {
        //            listOrder =  await query
        //                .Take(size + 1)
        //                .Select(o => new Models.Order
        //                {
        //                    OrderDate = o.OrderDate,
        //                    OrderID = o.OrderID,
        //                    UserId = o.UserId,
        //                    CustomerName = o.CustomerName,
        //                    DriverName = o.DriverName,
        //                    TruckNo = o.TruckNo,
        //                    RmoocNo = o.RmoocNo,
        //                    ContainerNo = o.ContainerNo,
        //                    FromLocationName = o.FromLocationName,
        //                    ToLocationName = o.ToLocationName,
        //                    FromWhereName = o.FromWhereName,
        //                    Status = o.Status,
        //                    TotalCost = o.TotalCost
        //                })
        //                .ToListAsync();
        //        }
        //        // xử lí check xem có trang kế tiếp không
        //        bool nextPage = listOrder.Count > size ; 
        //        listOrder = nextPage ? listOrder.Take(size).ToList() : listOrder;   
                    
        //        return Ok(new ApiResponse<Object>
        //        {
        //            statusCode = 200,
        //            Message = "Lấy danh sách đơn hàng thành công",
        //            Data =  listOrder 
        //            //Data = new { listOrder , nextPage } 
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
        //    }
        //}



        
        [JwtAuthMiddleware]
        [HttpGet("detailOrder")]
         public async Task<IActionResult> DetailOrder([FromQuery] int id)
        {
            try
            {

                var orderRecord = await _context.Orders
                    .Where(o => o.OrderID == id && !o.IsDelete)
                    .Select(o => new UpdateOrderDTO    // ✅ Select sau
                    {
                        OrderDate = o.OrderDate.ToString(),
                        CustomerId = o.CustomerId,
                        CustomerName = o.CustomerName,
                        DriverId = o.DriverId ,
                        DriverName = o.DriverName,
                        TruckId = o.TruckId,
                        TruckNo = o.TruckNo,
                        RmoocId = o.RmoocId,
                        RmoocNo = o.RmoocNo,
                        ContainerNo = o.ContainerNo,
                        ContainerType = o.ContainerType,
                        BillBookingNo = o.BillBookingNo,
                        FromLocationID = (int)o.FromLocationId,
                        FromWhereID = (int)o.FromWhereId,
                        ToLocationID = (int)o.ToLocationId,
                        FromLocationName = o.FromLocationName,
                        FromWhereName = o.FromWhereName,
                        ToLocationName = o.ToLocationName,
                        Status = o.Status,
                        CreatedDate = o.CreatedDate,
                        OrderLineList1 = o.OrderLines.Select(ol => new OrderLineItemDTO
                        {
                            OrderLineId = ol.OrderLineId,
                            OrderId = ol.OrderId,
                            ItemID = ol.ItemId,
                            OrderLineItem = new CreateItemDTO
                            {
                                ItemName = ol.Item.ItemName,        // ✅ Sẽ work
                                FixedPrice = (int)ol.Item.FixedPrice, // ✅ Sẽ work
                                DisplayOrder = ol.Item.DisplayOrder  // ✅ Sẽ work
                            },
                            ItemDescription = ol.ItemDescription,
                            ItemCost = (int?)ol.ItemCost,
                            hasInvoice = ol.HasInvoice,
                            InvoiceName = ol.InvoiceName,
                            InvoiceNo = ol.InvoiceNo,
                            isActive = ol.IsActive
                        }).ToList(),
                        OrderImageList = o.Images.Where( img => img.isActive == true ).Select( img => new Image
                        {
                            ImageID = img.ImageID,
                            UserID = img.UserID,
                            FileName = img.FileName,
                            Descrip = img.Descrip,
                            Created = img.Created,
                            URL = img.URL,
                        }).ToList(),
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (orderRecord == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️Không tìm thấy Đơn hàng có ID : {id} .", Data = null });
                }

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Thông tin chi tiết đơn hàng có ID : '{id}' ",
                    Data = orderRecord
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpPut("updateOrder")]
        public async Task<IActionResult> UpdateOrder(
            [FromBody] UpdateOrderDTO orderDTO,
            [FromQuery] int id
        )
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userData = HttpContext.Items["User"] as UserClaimsDTO;
                int userID = userData.UserID;

                var validate = ValidateString.StringValidator(new Dictionary<string, string>
                {
                    {"Ngày đặt hàng" , orderDTO.OrderDate },
                    {"ID khách hàng" , orderDTO.CustomerId.ToString() },
                    {"Name khách hàng" , orderDTO.CustomerName },
                    {"ID tài xế" , orderDTO.DriverId.ToString() },
                    {"Tên tài xế" , orderDTO.DriverName },
                    {"ID Container" , orderDTO.TruckId.ToString() },
                    {"Biển số Container" , orderDTO.TruckNo },
                    {"ID Rmooc" , orderDTO.RmoocId.ToString() },
                    {"Biển số Rmooc" , orderDTO.RmoocNo },
                    {"Số Container" , orderDTO.ContainerNo },
                    {"Loại Container" , orderDTO.ContainerType },
                    {"Số Billing Booking" , orderDTO.BillBookingNo },
                    {"ID Cảng nhận Cont" , orderDTO.FromLocationID.ToString() },
                    {"ID Kho nhận/giao hàng" , orderDTO.FromWhereID.ToString() },
                    {"ID Cảng hạ/trả Cont" , orderDTO.ToLocationID.ToString() },
                    {"Cảng nhận Cont" , orderDTO.FromLocationName },
                    {"Kho nhận/giao hàng" , orderDTO.FromWhereName },
                    {"Cảng hạ/trả Cont" , orderDTO.ToLocationName },
                });
                if (validate != null)
                {
                    return Ok(validate);
                }

                var customerExistsTask = await _context.Customers.AnyAsync(c => c.IsActive && c.CustomerID == orderDTO.CustomerId);

                var driverExistsTask = await _context.Drivers.AnyAsync(d => d.IsActive && d.DriverID == orderDTO.DriverId);
                var driverStatus = await _context.Drivers.AnyAsync(d => d.IsActive && d.DriverID == orderDTO.DriverId && d.Status == Driver.StatusDriver.Available);

                var truckExistsTask = await _context.Trucks.AnyAsync(t => t.TruckType == 1 && t.IsActive && t.TruckID == orderDTO.TruckId);

                var rmoocExistsTask = await _context.Trucks.AnyAsync(t => t.TruckType == 2 && t.IsActive && t.TruckID == orderDTO.RmoocId);

                var fromPortExistsTask = await _context.Locations.AnyAsync(l => l.IsActive && l.LocationId == orderDTO.FromLocationID);

                var fromWarehouseExistsTask = await _context.Locations.AnyAsync(l => l.IsActive && l.LocationId == orderDTO.FromWhereID);

                var toPortExistsTask = await _context.Locations.AnyAsync(l => l.IsActive && l.LocationId == orderDTO.ToLocationID);
                if (!customerExistsTask)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "CustomerID không tồn tại", Data = null });
                if (!driverExistsTask)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "DriverID không tồn tại.", Data = null });
                if (!driverStatus)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Driver này đang bận giao đơn hàng khác", Data = null });
                if (!truckExistsTask)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "TruckID không tồn tại", Data = null });
                if (!rmoocExistsTask)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "RmoocID không tồn tại", Data = null });
                if (!fromPortExistsTask)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Kho nhận Container không tồn tại", Data = null });
                if (!fromWarehouseExistsTask)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Kho nhận Container không tồn tại", Data = null });
                if (!toPortExistsTask)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Cảng hạ Container không tồn tại", Data = null });


                var orderRecord = await _context.Orders
                    .Where(o => o.OrderID == id && !o.IsDelete)
                    .FirstOrDefaultAsync();
                if (orderRecord == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Không tìm thấy Đơn hàng này", Data = null });
                }
                if (!DateTime.TryParse(orderDTO.OrderDate, out var parsedDate))
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Định dạng ngày không hợp lệ", Data = null });
                }
                if (parsedDate.Date < DateTime.Now.Date)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Ngày đặt hàng ở quá khứ", Data = null });
                }

                orderRecord.OrderDate = parsedDate;
                orderRecord.UserId = userID;
                orderRecord.CustomerId = orderDTO.CustomerId;
                orderRecord.CustomerName = orderDTO.CustomerName;
                orderRecord.DriverId = orderDTO.DriverId;
                orderRecord.DriverName = orderDTO.DriverName;
                orderRecord.TruckId = orderDTO.TruckId;
                orderRecord.TruckNo = orderDTO.TruckNo;
                orderRecord.RmoocId = orderDTO.RmoocId;
                orderRecord.RmoocNo = orderDTO.RmoocNo;
                orderRecord.ContainerNo = orderDTO.ContainerNo;
                orderRecord.ContainerType = orderDTO.ContainerType;
                orderRecord.BillBookingNo = orderDTO.BillBookingNo;
                orderRecord.FromLocationId = orderDTO.FromLocationID;
                orderRecord.FromWhereId = orderDTO.FromWhereID;
                orderRecord.ToLocationId = orderDTO.ToLocationID;
                orderRecord.FromLocationName = orderDTO.FromLocationName;
                orderRecord.FromWhereName = orderDTO.FromWhereName;
                orderRecord.ToLocationName = orderDTO.ToLocationName;
                orderRecord.UpdatedDate = DateTime.Now;

                _context.Orders.Update(orderRecord);

                var list = new List<OrderLine>();

                int totalCost = 0;
                if (orderDTO.OrderLineList.Count == 0)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "OrderLine rỗng", Data = null });
                }
                var listOrderLineExist = await _context.OrderLines.Where(ol => ol.OrderId == orderRecord.OrderID).ToListAsync();
                var orderLineDtoDict = orderDTO.OrderLineList.ToDictionary(dto => dto.OrderLineId);
                var updatelistOrderLine = listOrderLineExist
                .Select(item =>
                {
                    if (orderLineDtoDict.TryGetValue(item.OrderLineId, out var dto))
                    {
                        item.OrderLineId = dto.OrderLineId;
                        item.ItemId = dto.ItemID;
                        item.ItemDescription = dto.ItemDescription;
                        item.ItemCost = dto.ItemCost;
                        item.HasInvoice = dto.hasInvoice;
                        item.InvoiceName = dto.InvoiceName;
                        item.InvoiceNo = dto.InvoiceNo;
                        item.IsActive = dto.isActive;
                        totalCost = (int)(totalCost + item.ItemCost);
                    }
                    return item;
                })
                .ToList();
                if (updatelistOrderLine.Any())
                {
                    _context.UpdateRange(updatelistOrderLine);
                }
                orderRecord.TotalCost = totalCost;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"✅ Cập nhật đơn hàng thành công - OrderID : '{orderRecord.OrderID}' ",
                    Data = null
                });

            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return Ok(new ApiResponse<string> { statusCode = 400, Message = "Order đã bị người khác cập nhật. Vui lòng load lại trước khi sửa.", Data = null });
            }
            catch (DbUpdateException ex) // More general database update exception
            {
                await transaction.RollbackAsync();
                return Ok(new ApiResponse<string>
                {
                    statusCode = 400,
                    Message = "Lỗi khi tạo đơn hàng. Vui lòng thử lại.",
                    Data = null
                });
            }
        }


        [JwtAuthMiddleware]
        [HttpPost("exportOrderDetail")]
        public async Task<IActionResult> ExportOrderDetail([FromQuery] int orderID, [FromQuery] string typeExport)
        {
            try
            {
                var FileName = $"OrderDetail_{orderID}.{typeExport}";
                var fileData = await _orderService.ExportOrderDetailAsync(orderID, typeExport);// csv , pdf
                return File(fileData, "application/pdf", FileName);
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }
        [JwtAuthMiddleware]
        [HttpPost("exportOrders")]
        public async Task<IActionResult> ExportOrders([FromBody] ExportRequestDTO requestDTO, [FromQuery] string typeExport)
        {
            try
            {
                var FileName = $"Orders_{DateTime.Now:yyyyMMdd_HHmmss}.{typeExport}";
                var fileData = await _orderService.ExportOrdersAsync(requestDTO, typeExport); // csv , pdf
                return File(fileData, "text/csv", FileName);
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthMiddleware]
        [HttpPut("updateStatusOrderForOperator")]
        public async Task<IActionResult> UpdateStatusOrderForOperator(
           [FromQuery] int orderID
        )
        {
            try
            {
                var dataUser = HttpContext.Items["User"] as UserClaimsDTO;
                int userID = dataUser?.UserID ?? 0;
                string statusString = "InProgress";
                var detailOrder = await _context.Orders
                    .Where(o => o.IsDelete == false && o.OrderID == orderID)
                    .FirstOrDefaultAsync();

                if (!Enum.TryParse<Order.OrderStatus>(statusString, true, out var status))
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"Trạng thái không hợp lệ: {statusString}", Data = null });
                }
                if (detailOrder == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Đơn hàngg không tìm thấy", Data = null });
                }
                if (detailOrder.Status == status)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Đơn hàng đã ở trạng thái hiện tại", Data = null });
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
                detailOrder.UserId = userID;
                detailOrder.UpdatedDate = DateTime.Now;

                

                _context.Orders.Update(detailOrder);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Cập nhật trạng thái đơn hàng thành công",
                    Data = orderID
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpPut("updateStatusOrderForAccountant")]
        public async Task<IActionResult> UpdateStatusOrderForAccountant(
           [FromQuery] int orderID
       )
        {
            try
            {
                var dataUser = HttpContext.Items["User"] as UserClaimsDTO;
                int userID = dataUser?.UserID ?? 0;
                string statusString = "AwaitingApproval";
                var detailOrder = await _context.Orders
                    .Where(o => o.IsDelete == false && o.OrderID == orderID)
                    .FirstOrDefaultAsync();

                if (!Enum.TryParse<Order.OrderStatus>(statusString, true, out var status))
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"Trạng thái không hợp lệ: {statusString}", Data = null });
                }
                if (detailOrder == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Đơn hàngg không tìm thấy", Data = null });
                }
                if (detailOrder.Status == status)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Đơn hàng đã ở trạng thái hiện tại", Data = null });
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
                detailOrder.UserId = userID;
                detailOrder.UpdatedDate = DateTime.Now;

                _context.Orders.Update(detailOrder);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Cập nhật trạng thái đơn hàng thành công",
                    Data = orderID
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        [JwtAuthMiddleware]
        [HttpPut("updateStatusOrderForApprover")]
        public async Task<IActionResult> UpdateStatusOrderForApprover(
           [FromQuery] int orderID
        )
        {
            try
            {
                var dataUser = HttpContext.Items["User"] as UserClaimsDTO;
                int userID = dataUser?.UserID ?? 0;
                string statusString = "Completed";
                var detailOrder = await _context.Orders
                    .Where(o => o.IsDelete == false && o.OrderID == orderID)
                    .FirstOrDefaultAsync();

                if (!Enum.TryParse<Order.OrderStatus>(statusString, true, out var status))
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"Trạng thái không hợp lệ: {statusString}", Data = null });
                }
                if (detailOrder == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Đơn hàngg không tìm thấy", Data = null });
                }
                if (detailOrder.Status == status)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Đơn hàng đã ở trạng thái hiện tại", Data = null });
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
                detailOrder.UserId = userID;
                detailOrder.UpdatedDate = DateTime.Now;

                _context.Orders.Update(detailOrder);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = "Cập nhật trạng thái đơn hàng thành công",
                    Data = orderID
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }

    }

}
