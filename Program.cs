using System.Text.Json.Serialization;
using WebApplication3.Configurations;
using WebApplication3.Services;
using WebApplication3.Services.DataAccessLayer.EfCore;
using WebApplication3.Services.Interfaces.EntityInterfaces;
using WebApplication3.Services.Services.Activitys;
using WebApplication3.Services.Services.Customers;
using WebApplication3.Services.Services.DashBoards;
using WebApplication3.Services.Services.Orders;
using WebApplication3.Services.Services.Photos;
using WebSocketManager = WebApplication3.Services.Services.DashBoards.WebSocketManager;


var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureCors();



// Custom Repository cho Customer (specific registration)
builder.Services.AddScoped<ICustomerDal, EfCustomerDal>(); // truy vấn database bằng Ef và hàm riêng biệt của Customer
builder.Services.AddScoped<ICustomerService, CustomerService>();// Customer Service sẽ dùng repository ICustomerDal để truy vấn db


builder.Services.AddScoped<IPhotoDal, EfPhotoDal>();
builder.Services.AddScoped<IOrderDal, EfOrderDal>();
builder.Services.AddScoped<IPhotoRepository, CloudinaryPhotoRepository>();
builder.Services.AddScoped<IImageService, ImageManager>();


builder.Services.AddScoped<IActivityService, ActivityManager>();
builder.Services.AddScoped<IActivityDal, EfActivityDal>();


builder.Services.AddScoped<IOrderDal, EfOrderDal>();
//builder.Services.AddScoped<IOrderExporter, OrderExportService>();
builder.Services.AddScoped<IOrderService, OrderExportManager>();


builder.Services.AddScoped<ITruckStatisticsService, TruckStatisticService>();
builder.Services.AddSingleton<WebSocketManager>();
// Background Service - Query mỗi 5s và broadcast
builder.Services.AddHostedService<TruckStatisticsBroadcastService>();



// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddSwaggerWithBearer(builder.Configuration);

//builder.Services.AddScoped<JwtService>();
builder.Services.AddSingleton<JwtService>();


// Swagger + context accessor
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();
app.UseWebSockets(); // <-- Cần có dòng này!
app.UseCors("AllowSpecificOrigin");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();     //kiểm tra token/người dùng.
//app.UseMiddleware<JwtAuthMiddleware>();
app.UseAuthorization(); //kiểm tra quyền.

app.MapControllers();  //map endpoint.

app.Run();

