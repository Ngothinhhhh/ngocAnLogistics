using WebApplication3.DTOs.ExportDTOs;

namespace WebApplication3.Features.ExportOrders.ExporterImplement.PdfExportFamily
{
    public interface IOrderExporter
    {
        Task<byte[]> ExportOrders(List<OrderExport> orderExports);

    }
}
