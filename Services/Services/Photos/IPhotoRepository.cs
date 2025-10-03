using Microsoft.AspNetCore.Mvc;
using WebApplication3.DTOs.ImageDTOs;
using WebApplication3.Models;

namespace WebApplication3.Services.Services.Photos
{
    public interface IPhotoRepository
    {
        Task<Image> AddPhotoForUserAsync(int userId, PhotoForCreationDTO photoDto);  // at Service : Cloudinary, S3,....

        //Task<Image> UploadFileAsync(int userId, IFormFile file); // at server

        //Task<Image> AddPhotoForUserAsync(int userId, PhotoForCreationDTO photoDto);

    }
}
