using System.Linq.Expressions;
using WebApplication3.Models.Entities;

namespace WebApplication3.Services.Interfaces
{
    public interface IEntityRepository<T> where T : class , IEntity, new () // ràng buộc - generic constraint - luật khi implement : phải là 1 class, T phải implement interface IEntity (ví dụ có ID hoặc các thuojc tính define trong đó
                                              // class T phải có contructor rỗng để khi cần có thể new ()
    {
        Task<List<T>> GetAll(Expression<Func<T, bool>> filter = null);

        Task<T> Get(Expression<Func<T, bool>> filter);
        Task<Boolean> Exist(Expression<Func<T, bool>> filter);
         
        Task Add(T entity); 
        Task Update(T entity);  
        Task Delete(T entity);  

    }
}
