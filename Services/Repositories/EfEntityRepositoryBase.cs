using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebApplication3.Datas;
using WebApplication3.Models.Entities;
using WebApplication3.Services.Interfaces;

namespace WebApplication3.Services.Repositories
{                                                                   // viết lại giống Interface implement vì phải chặt hơn hoặc áp dụng quy định cũ
    public class EfEntityRepositoryBase<TEntity, TContext> : IEntityRepository<TEntity> // TEntity : generic type parameter tự đặt ra cho các Models.
                                                                                        // TContext : generic type parameter tự đặt ra cho các DbContext
                                                                                        // => Đây là 2 pattern EF Core chuẩn, cứ làm theo.
        where TEntity : class, IEntity, new()
        where TContext : FleetDB
    {
        protected readonly TContext _context;
        public EfEntityRepositoryBase(TContext context)
        {
            _context = context;
        }

        public async Task Add(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);  
            await _context.SaveChangesAsync();
        }

        public async Task Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();  
        }

        public async Task<Boolean> Exist(Expression<Func<TEntity, bool>> filter)
        {
            if (filter == null)
            {
                return await _context.Set<TEntity>().AnyAsync();
            }
            return await _context.Set<TEntity>().AnyAsync(filter);  
        }

        public async Task<TEntity?> Get(Expression<Func<TEntity, bool>> filter) // Expression<Func<TEntity, bool>> kiểu delegate để dịch qua câu truy vấn SQL,
                                                                                // còn nếu <Func<TEntity, bool>>  thì ko dịch được
        { 
            return await _context.Set<TEntity>().FirstOrDefaultAsync(filter);
        }

        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> filter = null)
        {
            if (filter == null)
            {
                return await _context.Set<TEntity>().ToListAsync();
            }
            return await _context.Set<TEntity>().Where(filter).ToListAsync();
        }

        public async Task Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);   // chỉ đánh dấu là Modified
            await _context.SaveChangesAsync();        // mới lưu thật sự
        }
    }
}
