using WebApplication3.DTOs;
using WebApplication3.Models;

namespace WebApplication3.Services.Interfaces.EntityInterfaces
{
    public interface IPhotoDal : IEntityRepository<Image>
    {
        //Task<string> UploadFileAsync(IFormFile file);
        //Task<bool> DeleteFileAsync(int fileID);
        Task<ApiResponse<Object>> SoftDeleteImage(int imageID);    
    }
}
