using WebApplication3.DTOs.ExportDTOs;
using WebApplication3.Models;

namespace WebApplication3.Features.ExportOrders.ExporterImplement.CsvExportFamily
{
    public interface IOrderDetailExporter
    {
        Task<Byte[]> ExportOrderDetail(OrderExport orderExport);
    }
}
