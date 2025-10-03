using Microsoft.EntityFrameworkCore;
using Mysqlx.Crud;
using System.Globalization;
using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.DTOs.DashBoardDTOs;
using WebApplication3.DTOs.ExportDTOs;
using WebApplication3.DTOs.ItemDTOs;
using WebApplication3.DTOs.OrderDTOs;
using WebApplication3.DTOs.OrderLineDTOs;
using WebApplication3.DTOs.RequestDTOs;
using WebApplication3.Models;
using WebApplication3.Services.Interfaces.EntityInterfaces;
using WebApplication3.Services.Repositories;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Order = WebApplication3.Models.Order;

namespace WebApplication3.Services.DataAccessLayer.EfCore
{
    public class EfOrderDal : EfEntityRepositoryBase<Order, FleetDB>, IOrderDal
    {
        public EfOrderDal(FleetDB context) : base(context)
        {
        }

        public async Task<OrderExport> GetOrderDetail(int orderID)
        {
            var emptyOrderLineItem = new List<OrderLineItem>
            {
                new OrderLineItem
                {
                    ItemId = 0,
                    ItemCost = 0,
                    HasInvoice = false,
                    InvoiceName = "Rỗng",
                    InvoiceNo = "Rỗng",
                    ItemID = 0,
                    ItemName = "Rỗng",
                    FixedPrice = 0,
                }
            };

            var orderRecord = await _context.Orders
                    .Where(o => o.OrderID == orderID && !o.IsDelete)
                    .Select(o => new OrderExport    
                    {
                        OrderID = orderID,  
                        UserId = o.UserId,
                        OrderDate = o.OrderDate,
                        CustomerName = o.CustomerName,
                        DriverName = o.DriverName,
                        TruckNo = o.TruckNo,
                        RmoocNo = o.RmoocNo,
                        ContainerNo = o.ContainerNo,
                        ContainerType = o.ContainerType,
                        BillBookingNo = o.BillBookingNo,
                        FromLocationName = o.FromLocationName,
                        FromWhereName = o.FromWhereName,
                        ToLocationName = o.ToLocationName,
                        Status = o.Status,
                        CreatedDate = o.CreatedDate,
                        TotalCost = o.TotalCost,
                        OrderLineList = o.OrderLines.Any(ol => ol.IsActive == true)
                            ? o.OrderLines.Where(ol => ol.IsActive == true).Select(ol => new OrderLineItem
                            {
                                ItemId = ol.ItemId,
                                ItemCost = ol.ItemCost,
                                HasInvoice = ol.HasInvoice,
                                InvoiceName = ol.InvoiceName,
                                InvoiceNo = ol.InvoiceNo,
                                ItemID = ol.Item.ItemID,
                                ItemName = ol.Item.ItemName,
                                FixedPrice = (int)ol.Item.FixedPrice,
                            }).ToList()
                            : emptyOrderLineItem
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            return orderRecord;

        }

        public async Task<List<OrderExport>> GetOrdersExportAsync(ExportDataDTO request)
        {
            var searchKey = request.searchKey;
            var fromDate = request.fromDateStr;
            var toDate = request.toDateStr;
            var statusEnum = request.status;
            var sortBy = request.sortBy;    
            var order = request.order;
            var pageSize = request.pageSize;
            var pageNumber = request.pageNumber;    

            var query =  _context.Orders.Where(o => o.IsDelete == false);
            if (!string.IsNullOrEmpty(searchKey))
            {
                query = query.Where(o =>
                       o.OrderID.ToString().Contains(searchKey)
                    || o.ContainerNo.Contains(searchKey)
                    || o.BillBookingNo.Contains(searchKey)
                    || o.CustomerName.Contains(searchKey)
                    || o.TruckId.ToString().Contains(searchKey)
                    || o.RmoocNo.Contains(searchKey)
                    || o.FromLocationName.Contains(searchKey)
                    || o.ToLocationName.Contains(searchKey)
                    || o.FromWhereName.Contains(searchKey));
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

            var emptyOrderLineItem = new List<OrderLineItem>
            {
                new OrderLineItem
                {
                    ItemId = 0,
                    ItemCost = 0,
                    HasInvoice = false,
                    InvoiceName = "Rỗng",
                    InvoiceNo = "Rỗng",
                    ItemID = 0,
                    ItemName = "Rỗng",
                    FixedPrice = 0,
                }
            };


            var listOrderExport = await query
                .Skip((page - 1) * size)
                .Take(size)
                .Select(o => new OrderExport
                {
                    OrderDate = o.OrderDate,
                    OrderID = o.OrderID,
                    UserId = o.UserId,
                    CustomerId = o.CustomerId,
                    CustomerName = o.CustomerName,
                    DriverId = o.DriverId,  
                    DriverName = o.DriverName,
                    TruckId = o.TruckId,
                    RmoocId = o.RmoocId,
                    TruckNo = o.TruckNo,
                    RmoocNo = o.RmoocNo,
                    ContainerNo = o.ContainerNo,
                    ContainerType = o.ContainerType,
                    BillBookingNo = o.BillBookingNo,
                    FromLocationName = o.FromLocationName,
                    FromLocationId = o.FromLocationId,  
                    ToLocationName = o.ToLocationName,
                    ToLocationId = o.ToLocationId,
                    FromWhereName = o.FromWhereName,
                    FromWhereId = o.FromWhereId,
                    CreatedDate = o.CreatedDate,
                    Status = o.Status,
                    TotalCost = o.TotalCost,
                    OrderLineList = o.OrderLines.Any(ol => ol.IsActive == true)
                    ? o.OrderLines.Where(ol => ol.IsActive == true).Select(ol => new OrderLineItem
                    {
                        ItemId = ol.ItemId,
                        ItemCost = ol.ItemCost,
                        HasInvoice = ol.HasInvoice,
                        InvoiceName = ol.InvoiceName,
                        InvoiceNo = ol.InvoiceNo,
                        ItemID = ol.Item.ItemID,
                        ItemName = ol.Item.ItemName,
                        FixedPrice = (int)ol.Item.FixedPrice,
                    }).ToList()
                    : emptyOrderLineItem
                })
                .ToListAsync();
            return listOrderExport;
        }


        public async Task<ResponseDashBoard> GetStatisticsByDateRangeAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Orders.Where(o => o.IsDelete == false && o.Status == Order.OrderStatus.Completed);
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= toDate.Value);
            }
            var totalCompletedOrder = await query.CountAsync();
            var truckTrips = await query.GroupBy(o => new { o.TruckId, o.TruckNo })
                .Select(g => new TruckTripDTO
                {
                    truckID = g.Key.TruckId ?? 0,
                    truckNo = g.Key.TruckNo ?? "",
                    countCompletedTrip = g.Count()
                }
            ).OrderByDescending(t => t.countCompletedTrip).ToListAsync();
            var mostTrip = truckTrips.FirstOrDefault();    
            var leastTrip = truckTrips.LastOrDefault();  
            var topTrucks = truckTrips.Take(10).ToList();
            return new ResponseDashBoard { totalCompletedOrder = totalCompletedOrder , topTrucks = topTrucks, leastTrip = leastTrip , mostTrip = mostTrip };
        }

        public async Task<ResponseDashBoard> GetCurrentMonthStatisticsAsync()
        {
            // từ đầu tháng tới cuối tháng- lấy ngày tháng hiện tại
            var now = DateTime.Now;
            var fromDate = new DateTime(now.Year, now.Month, 1);  // 01/10/2025
            var toDate = fromDate.AddMonths(1).AddDays(-1); // 31/10/2025

            var query = _context.Orders.Where(o => o.IsDelete == false && o.Status == Order.OrderStatus.Completed);
            query = query.Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate);
            
            var totalCompletedOrder = await query.CountAsync();
            var truckTrips = await query.GroupBy(o => new { o.TruckId, o.TruckNo }) 
                .Select(g => new TruckTripDTO
                {
                    truckID = g.Key.TruckId ?? 0,
                    truckNo = g.Key.TruckNo ?? "",
                    countCompletedTrip = g.Count()
                }
            ).OrderByDescending(t => t.countCompletedTrip).ToListAsync();
            var mostTrip = truckTrips.FirstOrDefault();
            var leastTrip = truckTrips.LastOrDefault();
            var topTrucks = truckTrips.Take(10).ToList();
            return new ResponseDashBoard { totalCompletedOrder = totalCompletedOrder, topTrucks = topTrucks, leastTrip = leastTrip, mostTrip = mostTrip };
        } 

    }
}
