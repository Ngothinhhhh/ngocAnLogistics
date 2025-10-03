using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using WebApplication3.DTOs;
using WebApplication3.DTOs.CloudinaryDTO;
using WebApplication3.DTOs.ImageDTOs;
using WebApplication3.Models;

namespace WebApplication3.Services.Services.Photos
{
    public class CloudinaryPhotoRepository : IPhotoRepository
    {
        public IConfiguration Configuration { get; }
        private CloudinarySettings _cloudinarySettings;
        private Cloudinary _cloudinary;

        public CloudinaryPhotoRepository(IConfiguration configuration)
        {
            Configuration = configuration;

            _cloudinarySettings = Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();

            Account account = new Account(_cloudinarySettings.CloudName,
                _cloudinarySettings.ApiKey,
                _cloudinarySettings.ApiSecret
                );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<Image> AddPhotoForUserAsync(int userId, PhotoForCreationDTO photoDto)
        {
            var file = photoDto.File;
            var uploadResult = new ImageUploadResult();
            if (photoDto?.File == null || photoDto.File.Length == 0)
            {
                throw new ArgumentException("File upload không hợp lệ");
            }

            string fileNameWithTimestamp = "";

            if (file.Length > 0)
            {
                // Create filename with format: order_OrderID_timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileExtension = Path.GetExtension(file.FileName);
                
                fileNameWithTimestamp = $"order_{photoDto.OrderID}_{timestamp}{fileExtension}";
              
                var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(fileNameWithTimestamp, stream)
                };
                uploadResult = await _cloudinary.UploadAsync(uploadParams);
                await stream.DisposeAsync();
            }

            // Enhanced error checking
            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");
            }
            if (string.IsNullOrEmpty(uploadResult.SecureUrl?.ToString()))
            {
                throw new Exception("Cloudinary không trả về URL. Upload result: " +
                    $"Status: {uploadResult.StatusCode}, " +
                    $"Error: {uploadResult.Error?.Message ?? "No error message"}");
            }
            if (photoDto.Descrip == null)
            {
                photoDto.Descrip = "Chưa có chú thích";
            }
            if (!int.TryParse(photoDto.OrderID, out var orderId))
            {
                throw new Exception($"Sai định dạng ID Order");
            }
            var photo = new Image()
            {
                FileName = fileNameWithTimestamp,
                URL = uploadResult.SecureUrl.ToString(),
                Descrip = photoDto.Descrip,
                UserID = userId,
                OrderID = orderId,
                Created = DateTime.Now
            };
            return photo;
        }

        //public Task<Image> UploadFileAsync(int userId, IFormFile file)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
