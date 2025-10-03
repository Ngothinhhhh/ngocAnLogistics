using WebApplication3.Features.ExportOrders.ExporterImplement.CsvExportFamily;
using WebApplication3.Features.ExportOrders.ExporterImplement.PdfExportFamily;

namespace WebApplication3.Features.ExportOrders.InterfaceOrderExporter
{
    public class PdfExportFactory : IExportFactory
    {
        public IOrderDetailExporter CreateOrderDetailExporter()
        {
            return new PdfOrderDetailExporter();
        }

        public IOrderExporter CreateOrderExporter()
        {
            return new PdfOrderExporter();
        }
    }
}
