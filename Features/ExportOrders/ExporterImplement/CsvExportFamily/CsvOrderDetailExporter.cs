using CsvHelper;
using System.Globalization;
using System.Text;
using WebApplication3.DTOs.ExportDTOs;

namespace WebApplication3.Features.ExportOrders.ExporterImplement.CsvExportFamily
{
    public class CsvOrderDetailExporter : IOrderDetailExporter
    {
        public Task<byte[]> ExportOrderDetail(OrderExport orderExport)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // Write CSV headers
            csv.WriteField("Mã đơn hàng");
            csv.WriteField("Ngày giao");
            csv.WriteField("Mã người tạo");
            csv.WriteField("Khách hàng");
            csv.WriteField("Tài xế");
            csv.WriteField("Biển số xe");
            csv.WriteField("Mã rmooc");
            csv.WriteField("Số Container");
            csv.WriteField("Loại Container");
            csv.WriteField("Bill/Booking");
            csv.WriteField("Điểm đi");
            csv.WriteField("Điểm đến");
            csv.WriteField("Nơi nhận cont");
            csv.WriteField("Trạng thái");
            csv.WriteField("Tổng chi phí thực");
            csv.WriteField("Giá thực tế");
            csv.WriteField("Có hóa đơn");
            csv.WriteField("Tên hóa đơn");
            csv.WriteField("Số hóa đơn");
            csv.WriteField("Tên chi phí");
            csv.WriteField("Giá định mức");
            csv.NextRecord();

            foreach (var item in orderExport.OrderLineList)
            {
                // Order information
                csv.WriteField(orderExport.OrderID);
                csv.WriteField(orderExport.OrderDate.ToString("M/d/yyyy"));
                csv.WriteField(orderExport.UserId);
                csv.WriteField(orderExport.CustomerName );
                csv.WriteField(orderExport.DriverName);
                csv.WriteField(orderExport.TruckNo);
                csv.WriteField(orderExport.RmoocNo);
                csv.WriteField(orderExport.ContainerNo);
                csv.WriteField(orderExport.ContainerType);
                csv.WriteField(orderExport.BillBookingNo);
                csv.WriteField(orderExport.FromLocationName);
                csv.WriteField(orderExport.ToLocationName);
                csv.WriteField(orderExport.FromWhereName);
                csv.WriteField(GetStatusText(orderExport.Status));
                csv.WriteField(FormatCurrencyNumber(orderExport.TotalCost));
                csv.WriteField(FormatCurrencyNumber(item.ItemCost));
                csv.WriteField(item.HasInvoice == true ? "Có" : "Không");
                csv.WriteField(item.InvoiceName);
                csv.WriteField(item.InvoiceNo);
                csv.WriteField(item.ItemName);
                csv.WriteField(FormatCurrencyNumber(item.FixedPrice));
                csv.NextRecord();
            }
            writer.Flush();
            return Task.FromResult(memoryStream.ToArray());
        }


        private string GetStatusText(object status)
        {
            if (status == null) return "";

            var statusStr = status.ToString();
            return statusStr switch
            {
                "Pending" => "Chờ xử lý",
                "InProgress" => "Đang xử lý",
                "PickedUp" => "Đã lấy hàng",
                "InTransit" => "Đang vận chuyển",
                "Delivered" => "Đã giao",
                "AwaitingApproval" => "Chờ duyệt",
                "Completed" => "Hoàn thành",
                "Cancelled" => "Đã hủy",
                "FailedDelivery" => "Giao thất bại",
                _ => status.ToString()
            };
        }
        private string FormatCurrencyNumber(decimal? amount)
        {
            if (!amount.HasValue) return "0 VNĐ";
            return amount.Value.ToString("#,##0") + "đ";
        }

    }
}
