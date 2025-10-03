using Microsoft.AspNetCore.Mvc.Filters;
using WebApplication3.DTOs.UserDTO;
using WebApplication3.Services;

namespace WebApplication3.Middlewares
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class JwtAuthDriver : Attribute, IAsyncActionFilter
    {
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

                var userData = decodedToken as Dictionary<string,string>;
                if (!userData.ContainsKey("RoleName") || userData["RoleName"] != "Driver")
                {
                    Console.WriteLine("NOT PASS");
                    context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.HttpContext.Response.WriteAsync("Status401Unauthorized!");
                    return;
                }
                Console.WriteLine("Passs");
                context.HttpContext.Items["driver"] = userData;
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
