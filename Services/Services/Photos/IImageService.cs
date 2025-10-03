using Microsoft.AspNetCore.Mvc;
using WebApplication3.DTOs;
using WebApplication3.DTOs.ImageDTOs;
using WebApplication3.Models;

namespace WebApplication3.Services.Services.Photos
{
    public interface IImageService
    {
        Task<ApiResponse<Object>> AddImageAsync(int userID, PhotoForCreationDTO photo);

    }
}
