using WebApplication3.DTOs.DashBoardDTOs;
using WebApplication3.DTOs.OrderDTOs;

namespace WebApplication3.Services.Services.DashBoards
{
    public interface ITruckStatisticsService
    {
        Task<ResponseDashBoard> GetCurrentMonthStatisticsAsync();  
        Task<ResponseDashBoard> GetStatisticsByDateRangeAsync(DateTime? fromDate, DateTime? toDate);  
    }
}
