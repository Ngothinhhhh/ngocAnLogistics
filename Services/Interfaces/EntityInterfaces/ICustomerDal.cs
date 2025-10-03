using WebApplication3.Models;
using WebApplication3.Services.Interfaces;

namespace WebApplication3.Services.Interfaces.EntityInterfaces
{
    public interface ICustomerDal : IEntityRepository<Customer> //extends
    {
        // Các phương thức đặc biệt chỉ dành cho Customer
        Task<List<Customer>> GetCustomersWithOrders();
    }
}
