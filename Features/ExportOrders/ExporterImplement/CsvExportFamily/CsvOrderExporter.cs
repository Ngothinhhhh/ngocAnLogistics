using System.Text;
using WebApplication3.DTOs.ExportDTOs;
using WebApplication3.Features.ExportOrders.ExporterImplement.PdfExportFamily;

namespace WebApplication3.Features.ExportOrders.ExporterImplement.CsvExportFamily
{
    public class CsvOrderExporter : IOrderExporter
    {
        public Task<Byte[]> ExportOrders(List<OrderExport> orderExports)
        {
            using var writer = new StringWriter();
            using var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            // Write header
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
            csv.WriteField("Có hóa đơn?");
            csv.WriteField("Tên hóa đơn");
            csv.WriteField("Số hóa đơn");
            csv.WriteField("Tên chi phí");
            csv.WriteField("Giá định mức");
            csv.NextRecord();

            // Write data
            foreach (var order in orderExports)
            {
                foreach (var orderLine in order.OrderLineList)
                {
                    csv.WriteField(order.OrderID);
                    csv.WriteField(order.OrderDate.ToString("yyyy-MM-dd"));
                    csv.WriteField(order.UserId);
                    csv.WriteField(order.CustomerName);
                    csv.WriteField(order.DriverName);
                    csv.WriteField(order.TruckNo);
                    csv.WriteField(order.RmoocNo);
                    csv.WriteField(order.ContainerNo);
                    csv.WriteField(order.ContainerType);
                    csv.WriteField(order.BillBookingNo);
                    csv.WriteField(order.FromLocationName);
                    csv.WriteField(order.ToLocationName);
                    csv.WriteField(order.FromWhereName);
                    csv.WriteField(GetStatusText(order.Status));
                    csv.WriteField(order.TotalCost);
                    csv.WriteField(orderLine.ItemCost);
                    csv.WriteField(orderLine.HasInvoice);
                    csv.WriteField(orderLine.InvoiceName);
                    csv.WriteField(orderLine.InvoiceNo);
                    csv.WriteField(orderLine.ItemName);
                    csv.WriteField(orderLine.FixedPrice);
                    csv.NextRecord();
                }
            }

            // Convert to byte array and return
            return Task.FromResult(Encoding.UTF8.GetBytes(writer.ToString()));
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


    }
}
