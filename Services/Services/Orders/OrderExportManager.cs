using WebApplication3.DTOs.ExportDTOs;
using WebApplication3.DTOs.RequestDTOs;
using WebApplication3.Features.ExportOrders;
using WebApplication3.Features.ExportOrders.InterfaceOrderExporter;
using WebApplication3.Models;
using WebApplication3.Services.Interfaces.EntityInterfaces;

namespace WebApplication3.Services.Services.Orders
{
    public class OrderExportManager : IOrderService
    {
        private readonly IOrderDal _orderDal;

        public OrderExportManager( IOrderDal orderDal)
        {
            _orderDal = orderDal;
        }

        public async Task<byte[]> ExportOrderDetailAsync(int orderID, string condition)
        {
            var OrderToExport = await _orderDal.GetOrderDetail(orderID);

            switch (condition.ToLower())
            {
                case "pdf":
                    IExportFactory pdfFactory = new PdfExportFactory();
                    var pdfExporter = pdfFactory.CreateOrderDetailExporter();
                    var PdfOrderDetail = await pdfExporter.ExportOrderDetail(OrderToExport);
                    return PdfOrderDetail;
                    break;
                default:
                    IExportFactory csvFactory = new CsvExportFactory();
                    var csvExporter = csvFactory.CreateOrderDetailExporter();
                    var CsvOrderDetail = await csvExporter.ExportOrderDetail(OrderToExport);
                    return CsvOrderDetail;
                    break;
            }
        }

        public async Task<byte[]> ExportOrdersAsync(ExportRequestDTO request , string condition)
        {
            ExportDataDTO data = new ExportDataDTO();
            // Kiểm tra và chuyển đổi chuỗi ngày từ
            if (!string.IsNullOrEmpty(request.fromDateStr) &&
                DateTime.TryParse(request.fromDateStr, out var parsedFromDate))
            {
                data.fromDateStr = parsedFromDate;
            }

            if (!string.IsNullOrEmpty(request.toDateStr) &&
                DateTime.TryParse(request.toDateStr, out var parsedToDate))
            {
                // lấy đến cuối ngày
                data.toDateStr = parsedToDate.Date.AddDays(1).AddSeconds(-1);
            }

            // Validation date range
            if (data.fromDateStr.HasValue && data.toDateStr.HasValue && data.fromDateStr.Value > data.toDateStr.Value)
            {
                //return new ApiResponse<Object>
                //{
                //    statusCode = 400,
                //    Message = "Ngày bắt đầu không thể lớn hơn ngày kết thúc",
                //    Data = null
                //};
            }

            if (!string.IsNullOrEmpty(request.status) && Enum.TryParse<Order.OrderStatus>(request.status, true, out Order.OrderStatus parsedStatus))
            {
                data.status = parsedStatus;
            }
            var listOrderToExport = await _orderDal.GetOrdersExportAsync(data);

            switch (condition.ToLower())
            {
                case "pdf":
                    IExportFactory pdfFactory = new PdfExportFactory();
                    var pdfExporter = pdfFactory.CreateOrderExporter();
                    var PdfOrder =  await pdfExporter.ExportOrders(listOrderToExport);
                    return PdfOrder;    
                    break;
                default:
                    IExportFactory csvFactory = new CsvExportFactory();
                    var csvExporter = csvFactory.CreateOrderExporter();
                    var CsvOrder = await csvExporter.ExportOrders(listOrderToExport);
                    return CsvOrder;
                    break;
            }
        }

        //public async Task<Byte[]> ExportOrdersToPDFAsync(ExportRequestDTO request)
        //{
        //    ExportDataDTO data = new ExportDataDTO();
        //    // Kiểm tra và chuyển đổi chuỗi ngày từ
        //    if (!string.IsNullOrEmpty(request.fromDateStr) &&
        //        DateTime.TryParse(request.fromDateStr, out var parsedFromDate))
        //    {
        //        data.fromDateStr = parsedFromDate;
        //    }

        //    if (!string.IsNullOrEmpty(request.toDateStr) &&
        //        DateTime.TryParse(request.toDateStr, out var parsedToDate))
        //    {
        //        // lấy đến cuối ngày
        //        data.toDateStr = parsedToDate.Date.AddDays(1).AddSeconds(-1);
        //    }

        //    // Validation date range
        //    if (data.fromDateStr.HasValue && data.toDateStr.HasValue && data.fromDateStr.Value > data.toDateStr.Value)
        //    {
        //        //return new ApiResponse<Object>
        //        //{
        //        //    statusCode = 400,
        //        //    Message = "Ngày bắt đầu không thể lớn hơn ngày kết thúc",
        //        //    Data = null
        //        //};
        //    }

        //    if (!string.IsNullOrEmpty(request.status) && Enum.TryParse<Order.OrderStatus>(request.status, true, out Order.OrderStatus parsedStatus))
        //    {
        //        data.status = parsedStatus;
        //    }
        //    var listOrderToExport = await _orderDal.GetOrdersExportAsync(data);
        //    var pdfBytes = _exportService.ExportOrdersToPDFAsync(listOrderToExport);

        //    return pdfBytes;
        //    //return new ApiResponse<Object> { Data = pdfBytes, Message = "Export to PDF thành công", statusCode = 200 };
        //}
    }
}
