using WebApplication3.Features.ExportOrders.ExporterImplement.CsvExportFamily;
using WebApplication3.Features.ExportOrders.ExporterImplement.PdfExportFamily;

namespace WebApplication3.Features.ExportOrders.InterfaceOrderExporter
{
    public class CsvExportFactory : IExportFactory
    {
        public IOrderDetailExporter CreateOrderDetailExporter()
        {
            return new CsvOrderDetailExporter();
        }

        public IOrderExporter CreateOrderExporter()
        {
            return new CsvOrderExporter();
        }
    }
}
