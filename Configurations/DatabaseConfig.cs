using Microsoft.EntityFrameworkCore;
using WebApplication3.Datas;
using MySqlConnector;

namespace WebApplication3.Configurations
{
    public static class DatabaseConfig
    {
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");


            services.AddTransient<MySqlConnection>(_ =>
                new MySqlConnection(connectionString));

            services.AddDbContext<FleetDB>(options =>
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 21))
                )
            );
            return services;
        }
        
    }
}
