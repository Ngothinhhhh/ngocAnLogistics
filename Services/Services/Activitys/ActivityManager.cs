using Microsoft.AspNetCore.Http;
using System.Net.Http;
using WebApplication3.DTOs;
using WebApplication3.DTOs.ActivityDTOs;
using WebApplication3.Helpers;
using WebApplication3.Models;
using WebApplication3.Services.DataAccessLayer.EfCore;
using WebApplication3.Services.Interfaces;
using WebApplication3.Services.Interfaces.EntityInterfaces;

namespace WebApplication3.Services.Services.Activitys
{
    public class ActivityManager : IActivityService
    {
        private readonly IActivityDal _activityDal;
        public ActivityManager(IActivityDal activityDal)
        {
            _activityDal = activityDal;
        }

        public async Task<ApiResponse<Object>> CreateActivity(ActivityForCreation activityDTO)
        {
                var validate = ValidateObjectDTO.StringValidator(new Dictionary<string, string>
                {
                    {"Mã đơn hàng" , activityDTO.OrderID.ToString() },
                    {"Tên trường thay đổi" , activityDTO.FieldName },
                    {"Thay đổi thành" , activityDTO.NewValue },
                    {"Thay đổi từ" , activityDTO.OldValue },
                    {"Địa chỉ IP" , activityDTO.IP },
                });
                if (validate != null)
                {
                    return validate;
                }

            var activity = new Activity()
                {
                    UserID = activityDTO.UserID,
                    OrderID = activityDTO.OrderID,
                    FieldName = activityDTO.FieldName,
                    OldValue = activityDTO.OldValue,
                    NewValue = activityDTO.NewValue,
                    ActivityDetail = activityDTO.ActivityDetail,
                    ActivityDate = DateTime.Now,
                    IP = activityDTO.IP   
                };
                await _activityDal.Add(activity);
                return new ApiResponse<Object> { Data = null, Message = "Activity Done", statusCode = 200 };
        }
    }
}
