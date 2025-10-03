using Microsoft.AspNetCore.Mvc.Filters;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Globalization;
using WebApplication3.DTOs.UserDTO;
using WebApplication3.Helpers;
using WebApplication3.Models;
using WebApplication3.Services;

namespace WebApplication3.Middlewares
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class JwtAuthMiddleware : Attribute , IAsyncActionFilter
    {
        //private readonly RequestDelegate _next;
        //private readonly JwtService _jwtService;

        public JwtAuthMiddleware()
        {
        }

        //public JwtAuthMiddleware( JwtService jwtService)
        //{ 
        //    //_next = next;
        //    _jwtService = jwtService;
        //}

        public async Task Invoke(HttpContext context)
        {
            
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            var _jwtService = context.HttpContext.RequestServices.GetRequiredService<JwtService>();
            /*
                HttpContext.RequestServices chính là service provider của request hiện tại.
                Bạn có thể gọi GetRequiredService<JwtService>() để lấy ra instance của JwtService đã đăng ký trong DI container, tương tự như khi inject vào Controller.
             ===> Không thể tiêm Object Service vào trong Attribute, chỉ có thể làm được khi làm ở Middlware.
             */

            // Lấy header Authorization
            string authHeader = context.HttpContext.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader))
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.HttpContext.Response.WriteAsync("Fill your token pls!");
                return;
            }
            if (!authHeader.StartsWith("Bearer "))
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.HttpContext.Response.WriteAsync("Invalid Authorization format. Must start with 'Bearer '.");
                return;
            }
            string token = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.HttpContext.Response.WriteAsync("Undefined Token!");
                return;
            }
            try
            {
                var decodedToken = _jwtService.DecodedToken(token);
                if (decodedToken == null)
                {
                    Console.WriteLine("NOT PASS");
                    context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.HttpContext.Response.WriteAsync("Error token!");
                    return;
                }

                var userData = decodedToken as Dictionary<string, string>;
                int? userId = null;
                if (userData.ContainsKey("UserID"))
                {
                    if (int.TryParse(userData["UserID"], out int parsedId))
                    {
                        userId = parsedId;
                    }
                }
                //DictionaryHelper.userDataPrintDictionary(userData);

                int? roleID = null;
                if (userData.ContainsKey("RoleID"))
                {
                    if (int.TryParse(userData["RoleID"], out int parsedId))
                    {
                        roleID = parsedId;
                    }
                }
                else
                {
                    Console.WriteLine("ko co gia tri");
                }

                var roleName = userData.ContainsKey("RoleName") ? userData["RoleName"] : null;
                var userName = userData.ContainsKey("UserName") ? userData["UserName"] : null;
                var fullname = userData.ContainsKey("FullName") ? userData["FullName"] : null;

                UserClaimsDTO userClaims = new UserClaimsDTO
                {
                    UserID   = (int)userId,
                    RoleName = roleName,
                    UserName = userName,
                    FullName = fullname,
                    RoleID   = (int)roleID

                };
                Console.WriteLine("Pass");
                context.HttpContext.Items["User"] = userClaims;
            
            }
            catch (Exception ex)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.HttpContext.Response.WriteAsync($"Error token: {ex.Message}");
                return;
            }
            // Cho request đi tiếp
            await next();
        }
    }
}
