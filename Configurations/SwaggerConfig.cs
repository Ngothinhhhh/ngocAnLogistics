using Microsoft.OpenApi.Models;

namespace WebApplication3.Configurations
{
    public static class SwaggerConfig
    {
        public static IServiceCollection AddSwaggerWithBearer(this IServiceCollection services, IConfiguration configuration)
        {
            // Có thể đọc từ appsettings.json
            var apiTitle = configuration["Swagger:Title"] ?? "Fleet API";
            var apiVersion = configuration["Swagger:Version"] ?? "v1";

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(apiVersion, new OpenApiInfo
                {
                    Title = apiTitle,
                    Version = apiVersion,
                    Description = "API Documentation with JWT Authentication"
                });

                // Thêm Bearer Auth
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token theo format: Bearer {token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }
    }

}
