using WebApplication3.Models;

namespace WebApplication3.DTOs.ExportDTOs
{
    public class ExportDataDTO
    {
        public DateTime? fromDateStr { get; set; }
        public DateTime? toDateStr { get; set; }
        public string order { get; set; } = "asc";
        public string sortBy { get; set; } = "id";
        public int pageSize { get; set; } = 30;
        public int pageNumber { get; set; } = 1;
        public string? searchKey { get; set; }
        public Order.OrderStatus? status { get; set; }

    }
}
