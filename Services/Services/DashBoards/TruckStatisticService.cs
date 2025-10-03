using WebApplication3.DTOs.DashBoardDTOs;
using WebApplication3.Services.DataAccessLayer.EfCore;
using WebApplication3.Services.Interfaces.EntityInterfaces;

namespace WebApplication3.Services.Services.DashBoards
{
    public class TruckStatisticService : ITruckStatisticsService
    {
        private readonly IOrderDal _orderDal;   
        public TruckStatisticService(IOrderDal orderDal)
        {
            _orderDal = orderDal;
        }
        public Task<ResponseDashBoard> GetCurrentMonthStatisticsAsync()
        {
            return _orderDal.GetCurrentMonthStatisticsAsync();
        }

        public Task<ResponseDashBoard> GetStatisticsByDateRangeAsync(DateTime? fromDate, DateTime? toDate)
        {
            return _orderDal.GetStatisticsByDateRangeAsync(fromDate, toDate); 
        }
    }
}
