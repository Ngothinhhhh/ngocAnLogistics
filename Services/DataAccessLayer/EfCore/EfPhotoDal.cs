using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.Models;
using WebApplication3.Services.Interfaces.EntityInterfaces;
using WebApplication3.Services.Repositories;

namespace WebApplication3.Services.DataAccessLayer.EfCore
{
    public class EfPhotoDal : EfEntityRepositoryBase<Image, FleetDB>, IPhotoDal
    {
        private readonly FleetDB _context;

        public EfPhotoDal(FleetDB context) : base(context)
        {
            _context = context;
        }        

        public async Task<ApiResponse<Object>> SoftDeleteImage(int imageID)
        {
            try
            {
                var entity = await _context.Image.FirstOrDefaultAsync(i => i.ImageID == imageID);
                if (entity == null)
                {
                    return new ApiResponse<Object> { statusCode = 400 , Message = "Không thấy Entity này" , Data = null} ;
                }
                entity.isActive = false;
                _context.Image.Update(entity);
                return new ApiResponse<Object> { statusCode = 200, Message = "Xóa mềm thành công", Data = null };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting file with ID {imageID}: {ex.Message}", ex);
            }
        }

        
    }
}