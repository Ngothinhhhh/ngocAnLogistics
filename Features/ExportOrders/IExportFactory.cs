using WebApplication3.Features.ExportOrders.ExporterImplement.CsvExportFamily;
using WebApplication3.Features.ExportOrders.ExporterImplement.PdfExportFamily;

namespace WebApplication3.Features.ExportOrders
{
    public interface IExportFactory
    {
        IOrderDetailExporter CreateOrderDetailExporter(); 
        IOrderExporter CreateOrderExporter(); 

    }
}
