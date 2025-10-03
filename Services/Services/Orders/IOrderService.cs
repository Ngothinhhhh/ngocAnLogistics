using WebApplication3.DTOs;
using WebApplication3.DTOs.RequestDTOs;

namespace WebApplication3.Services.Services.Orders
{
    public interface IOrderService
    {
        // csv
        // pdf
        public Task<byte[]> ExportOrdersAsync(ExportRequestDTO request, string condition);

        public Task<byte[]> ExportOrderDetailAsync(int OrderID, string condition);

    }
}
