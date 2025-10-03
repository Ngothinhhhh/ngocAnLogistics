using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.DTOs;
using WebApplication3.DTOs.CustomerDTOs;
using WebApplication3.DTOs.ImageDTOs;
using WebApplication3.DTOs.RequestDTOs;
using WebApplication3.DTOs.UserDTO;
using WebApplication3.Helpers;
using WebApplication3.Middlewares;
using WebApplication3.Models;
using WebApplication3.Services.Services.Orders;
using WebApplication3.Services.Services.Photos;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IOrderService _orderService;
        public ImageController(IImageService imageService , IOrderService orderService)
        {
            _imageService = imageService;
            _orderService = orderService;
        }

        [JwtAuthMiddleware]
        [HttpPost("createImageAndUpload")]
        public async Task<IActionResult> CreateImageAndUpload([FromForm] PhotoForCreationDTO photoDTO)
        {
            try
            { 
                var dataUser = HttpContext.Items["User"] as UserClaimsDTO ;
                if (dataUser == null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Chưa có thông tin về User này.", Data = null });
                }
                var userId = dataUser.UserID;
                var image = await _imageService.AddImageAsync(userId, photoDTO);
                return Ok(image);
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


    }
}
