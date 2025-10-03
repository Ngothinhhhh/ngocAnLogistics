using WebApplication3.Datas;
using WebApplication3.Models;
using WebApplication3.Services.Interfaces.EntityInterfaces;
using WebApplication3.Services.Repositories;

namespace WebApplication3.Services.DataAccessLayer.EfCore
{
    public class EfActivityDal : EfEntityRepositoryBase<Activity, FleetDB> , IActivityDal
    {
        public EfActivityDal(FleetDB context) : base(context)
        {
        }

    }
}
