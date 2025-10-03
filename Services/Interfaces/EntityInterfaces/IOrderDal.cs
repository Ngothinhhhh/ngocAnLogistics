using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.DTOs.DashBoardDTOs;
using WebApplication3.DTOs.ExportDTOs;
using WebApplication3.DTOs.OrderDTOs;
using WebApplication3.DTOs.RequestDTOs;
using WebApplication3.Models;
using WebApplication3.Models.Entities;
using WebApplication3.Services.Interfaces;

namespace WebApplication3.Services.Interfaces.EntityInterfaces
{
    public interface IOrderDal : IEntityRepository<Order>
    {
        Task<OrderExport> GetOrderDetail(int orderID);  
        Task<List<OrderExport>> GetOrdersExportAsync(ExportDataDTO request);

        Task<ResponseDashBoard> GetStatisticsByDateRangeAsync(DateTime? fromDate, DateTime? toDate);
        Task<ResponseDashBoard> GetCurrentMonthStatisticsAsync();
    }
}
