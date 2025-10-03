using WebApplication3.DTOs.CustomerDTOs;
using WebApplication3.Models;

namespace WebApplication3.Services.Services.Customers
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetActiveCustomers();

    }
}
