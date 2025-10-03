using Microsoft.EntityFrameworkCore;
using WebApplication3.Datas;
using WebApplication3.Models;
using WebApplication3.Services.Interfaces.EntityInterfaces;
using WebApplication3.Services.Repositories;

namespace WebApplication3.Services.DataAccessLayer.EfCore
{
    public class EfCustomerDal : EfEntityRepositoryBase<Customer, FleetDB>, ICustomerDal
    {
        public EfCustomerDal(FleetDB context) : base(context)
        {
        }

        public async Task<List<Customer>> GetCustomersWithOrders()
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .ToListAsync();
        }

    }
}
