using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using WebApplication3.DTOs.ExportDTOs;

namespace WebApplication3.Features.ExportOrders.ExporterImplement.PdfExportFamily
{
    public class PdfOrderExporter : IOrderExporter
    {
        public Task<byte[]> ExportOrders(List<OrderExport> orderExports)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Tạo font hỗ trợ tiếng Việt
            PdfFont font, boldFont, titleFont;
            try
            {
                // Thử dùng Arial trên Windows
                font = PdfFontFactory.CreateFont("c:/windows/fonts/arial.ttf", PdfEncodings.IDENTITY_H);
                boldFont = PdfFontFactory.CreateFont("c:/windows/fonts/arialbd.ttf", PdfEncodings.IDENTITY_H);
                titleFont = boldFont;
            }
            catch
            {
                try
                {
                    // Fallback: Thử Calibri
                    font = PdfFontFactory.CreateFont("c:/windows/fonts/calibri.ttf", PdfEncodings.IDENTITY_H);
                    boldFont = PdfFontFactory.CreateFont("c:/windows/fonts/calibrib.ttf", PdfEncodings.IDENTITY_H);
                    titleFont = boldFont;
                }
                catch
                {
                    // Fallback cuối: dùng font mặc định (không có dấu)
                    font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    titleFont = boldFont;
                }
            }

            foreach (var order in orderExports)
            {
                // Add new page for each order (except first)
                if (orderExports.IndexOf(order) > 0)
                {
                    document.Add(new AreaBreak());
                }

                // Order Header
                var orderTitle = new Paragraph($"CHI TIẾT ĐƠN HÀNG - MÃ ĐƠN #{order.OrderID}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFont(titleFont)
                    .SetFontSize(18)
                    .SetMarginBottom(20);
                document.Add(orderTitle);

                // Order Information Section
                var orderInfoTitle = new Paragraph("THÔNG TIN ĐƠN HÀNG")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetPadding(5);
                document.Add(orderInfoTitle);

                // Create order info table
                var orderInfoTable = new Table(new float[] { 1f, 2f, 1f, 2f });
                orderInfoTable.SetWidth(UnitValue.CreatePercentValue(100));
                orderInfoTable.SetMarginBottom(15);

                // Add order information fields
                AddInfoRow(orderInfoTable, "Mã đơn:", order.OrderID.ToString() ?? "", "Ngày giao:", order.OrderDate.ToString("dd/MM/yyyy") ?? "", font, boldFont);
                AddInfoRow(orderInfoTable, "Mã người tạo:", order.UserId.ToString() ?? "", "Trạng thái:", GetStatusText(order.Status), font, boldFont);
                AddInfoRow(orderInfoTable, "Khách hàng:", order.CustomerName ?? "", "Tài xế:", order.DriverName ?? "", font, boldFont);
                AddInfoRow(orderInfoTable, "Biển số xe:", order.TruckNo ?? "", "Mã rmooc:", order.RmoocNo ?? "", font, boldFont);
                AddInfoRow(orderInfoTable, "Số Container:", order.ContainerNo ?? "", "Loại Container:", order.ContainerType ?? "", font, boldFont);
                AddInfoRow(orderInfoTable, "Bill/Booking:", order.BillBookingNo ?? "", "Tổng chi phí:", FormatCurrency(order.TotalCost), font, boldFont);

                document.Add(orderInfoTable);

                // Location Information
                var locationTitle = new Paragraph("THÔNG TIN ĐỊA ĐIỂM")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetPadding(5);
                document.Add(locationTitle);

                var locationTable = new Table(new float[] { 1f, 2f, 1f, 2f });
                locationTable.SetWidth(UnitValue.CreatePercentValue(100));
                locationTable.SetMarginBottom(15);

                AddInfoRow(locationTable, "Điểm đi:", order.FromLocationName ?? "", "Điểm đến:", order.ToLocationName ?? "", font, boldFont);
                AddInfoRow(locationTable, "Từ nơi:", order.FromWhereName ?? "", "", "", font, boldFont);

                document.Add(locationTable);

                // Items Section
                if (order.OrderLineList != null && order.OrderLineList.Any())
                {
                    var itemsTitle = new Paragraph($"CHI TIẾT CHI PHÍ ({order.OrderLineList.Count} khoản)")
                        .SetFont(boldFont)
                        .SetFontSize(14)
                        .SetMarginBottom(10)
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetPadding(5);
                    document.Add(itemsTitle);

                    // Items table
                    var itemsTable = new Table(new float[] { 0.5f, 2f, 1.2f, 1.2f, 0.8f, 1.5f, 1f });
                    itemsTable.SetWidth(UnitValue.CreatePercentValue(100));

                    // Items table headers
                    var itemHeaders = new string[] { "STT", "Tên chi phí", "Giá thực tế", "Giá định mức", "Có hóa đơn", "Tên hóa đơn", "Số hóa đơn" };
                    foreach (var header in itemHeaders)
                    {
                        var headerCell = new Cell()
                            .Add(new Paragraph(header)
                                .SetFont(boldFont)
                                .SetFontSize(10)
                                .SetTextAlignment(TextAlignment.CENTER))
                            .SetBackgroundColor(ColorConstants.GRAY)
                            .SetPadding(5);
                        itemsTable.AddHeaderCell(headerCell);
                    }

                    // Add items data
                    int itemNo = 1;
                    foreach (var item in order.OrderLineList)
                    {
                        AddItemCell(itemsTable, itemNo.ToString(), font);
                        AddItemCell(itemsTable, item.ItemName ?? "", font);
                        AddItemCell(itemsTable, FormatCurrency(item.ItemCost), font);
                        AddItemCell(itemsTable, FormatCurrency(item.FixedPrice), font);
                        AddItemCell(itemsTable, item.HasInvoice == true ? "Có" : "Không", font);
                        AddItemCell(itemsTable, item.InvoiceName ?? "", font);
                        AddItemCell(itemsTable, item.InvoiceNo ?? "", font);
                        itemNo++;
                    }

                    document.Add(itemsTable);

                    // Items summary
                    var totalItemCost = order.OrderLineList.Sum(i => i.ItemCost ?? 0);
                    var totalFixedPrice = order.OrderLineList.Sum(i => i.FixedPrice);
                    var invoicedItems = order.OrderLineList.Count(i => i.HasInvoice == true);

                    var itemsSummary = new Paragraph("TỔNG KẾT CHI PHÍ:")
                        .SetFont(boldFont)
                        .SetFontSize(12)
                        .SetMarginTop(10);
                    document.Add(itemsSummary);

                    var summaryTable = new Table(new float[] { 1f, 1f, 1f, 1f });
                    summaryTable.SetWidth(UnitValue.CreatePercentValue(100));
                    summaryTable.SetMarginBottom(20);

                    AddSummaryRow(summaryTable, "Tổng số khoản:", order.OrderLineList.Count.ToString(), "Số khoản có hóa đơn:", invoicedItems.ToString(), font, boldFont);
                    AddSummaryRow(summaryTable, "Tổng chi phí thực tế:", FormatCurrency(totalItemCost), "Tổng chi phí định mức:", FormatCurrency(totalFixedPrice), font, boldFont);

                    document.Add(summaryTable);
                }

                // Order footer
                var footer = new Paragraph($"Được tạo lúc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFont(font)
                    .SetFontSize(8)
                    .SetFontColor(ColorConstants.GRAY)
                    .SetMarginTop(20);
                document.Add(footer);
            }

            // Close document to finalize the PDF
            document.Close();

            // Return the PDF as byte array
            return Task.FromResult(memoryStream.ToArray());
        }

        private string GetStatusText(object status)
        {
            if (status == null) return "";

            var statusStr = status.ToString().ToLower();
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

        private void AddInfoRow(Table table, string label1, string value1, string label2, string value2, PdfFont font, PdfFont boldFont)
        {
            table.AddCell(new Cell().Add(new Paragraph(label1).SetFont(boldFont).SetFontSize(10)).SetPadding(5));
            table.AddCell(new Cell().Add(new Paragraph(value1).SetFont(font).SetFontSize(10)).SetPadding(5));
            table.AddCell(new Cell().Add(new Paragraph(label2).SetFont(boldFont).SetFontSize(10)).SetPadding(5));
            table.AddCell(new Cell().Add(new Paragraph(value2).SetFont(font).SetFontSize(10)).SetPadding(5));
        }

        private void AddSummaryRow(Table table, string label1, string value1, string label2, string value2, PdfFont font, PdfFont boldFont)
        {
            table.AddCell(new Cell().Add(new Paragraph(label1).SetFont(boldFont).SetFontSize(9)).SetPadding(3));
            table.AddCell(new Cell().Add(new Paragraph(value1).SetFont(font).SetFontSize(9)).SetPadding(3));
            table.AddCell(new Cell().Add(new Paragraph(label2).SetFont(boldFont).SetFontSize(9)).SetPadding(3));
            table.AddCell(new Cell().Add(new Paragraph(value2).SetFont(font).SetFontSize(9)).SetPadding(3));
        }

        private void AddItemCell(Table table, string content, PdfFont font)
        {
            var cell = new Cell()
                .Add(new Paragraph(content)
                    .SetFont(font)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.LEFT))
                .SetPadding(4);
            table.AddCell(cell);
        }

        private string FormatCurrency(decimal? amount)
        {
            if (amount == null) return "0";
            return amount.Value.ToString("N0") + " VND";
        }
    }
}
