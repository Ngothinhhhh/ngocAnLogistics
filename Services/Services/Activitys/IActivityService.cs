using WebApplication3.DTOs;
using WebApplication3.DTOs.ActivityDTOs;

namespace WebApplication3.Services.Services.Activitys
{
    public interface IActivityService
    {
        Task<ApiResponse<Object>> CreateActivity( ActivityForCreation activityForCreation);
    }
}
