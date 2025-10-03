using System.Linq; // Thêm dòng này vào đầu file
using WebApplication3.DTOs.CustomerDTOs;
using WebApplication3.Models;
using WebApplication3.Services.Interfaces.EntityInterfaces;


namespace WebApplication3.Services.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerDal _customerDal; // tiêm interface ICustomerDal vào
        public CustomerService(ICustomerDal customerDal)
        {
            _customerDal = customerDal;  
        }

        //Các phương thức nghiệp vụ sử dụng; repository
        public async Task<List<Customer>> GetActiveCustomers()
        {
            var customers = await _customerDal.GetAll(c => c.IsActive);
            return customers.Select(c => new Customer
            {
                CustomerID = c.CustomerID,
                CustomerName = c.CustomerName,
                ContactPhone = c.ContactPhone,
                CustomerCode = c.CustomerCode,
            }).ToList();
        }



    }
}
