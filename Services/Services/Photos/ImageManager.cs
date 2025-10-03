using Microsoft.EntityFrameworkCore;
using WebApplication3.DTOs;
using WebApplication3.DTOs.ImageDTOs;
using WebApplication3.Helpers;
using WebApplication3.Models;
using WebApplication3.Services.Interfaces.EntityInterfaces;

namespace WebApplication3.Services.Services.Photos
{
    public class ImageManager : IImageService
    {
        private readonly IPhotoDal _photoDal;
        private readonly IPhotoRepository _photoRepository;
        private readonly IOrderDal _orderDal;

        public ImageManager(IPhotoDal photoDal , IPhotoRepository photoRepository, IOrderDal orderDal)
        {
            _photoDal = photoDal;
            _photoRepository = photoRepository;
            _orderDal = orderDal;
        }

        public async Task<ApiResponse<Object>> AddImageAsync(int userID , PhotoForCreationDTO photoDTO)
        {
            var validate = ValidateObjectDTO.StringValidator(new Dictionary<string, string>
            {
                {"Mã đơn hàng" , photoDTO.OrderID }
            });

            if (!int.TryParse(photoDTO.OrderID, out var orderID))
            {
                return new ApiResponse<Object> { statusCode = 400, Message = $"⚠️ID Đơn hàng sai định dạng.", Data = null };
            }

            if (validate != null)
            {
                return validate;
            }
            var existsOrder = await _orderDal.Exist(O => O.OrderID == orderID);
            if (!existsOrder) return new ApiResponse<Object> { statusCode = 400, Message = $"⚠️ Đơn hàng không tồn tại.", Data = null };

            var photoCreation = await _photoRepository.AddPhotoForUserAsync(userID, photoDTO);
            await _photoDal.Add(photoCreation);
            return new ApiResponse<Object>
            {
                statusCode = 200,
                Message = "Thêm ảnh thành công",
                Data = photoCreation
            };
        }
    }
}
