using WebApplication3.DTOs.OrderDTOs;

namespace WebApplication3.DTOs.DashBoardDTOs
{
    public class ResponseDashBoard
    {
        public int totalCompletedOrder { get; set; }
        public TruckTripDTO mostTrip { get; set; }
        public TruckTripDTO leastTrip { get; set; }
        public ICollection<TruckTripDTO> topTrucks { get; set; }
    }
}
