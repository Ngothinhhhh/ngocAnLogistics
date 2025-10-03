
using System.Text.Json;

namespace WebApplication3.Services.Services.DashBoards
{
    public class TruckStatisticsBroadcastService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider; // dùng tạo Scope mới mỗi Request nhằm lấy DbContext
        private readonly WebSocketManager _websocketManager;    
        private readonly ILogger<TruckStatisticsBroadcastService> _logger;
        private readonly IConfiguration configuration;
        private readonly int _intervalSeconds;

        public TruckStatisticsBroadcastService(IServiceProvider serviceProvider, WebSocketManager websocketManager, ILogger<TruckStatisticsBroadcastService> logger)
        {
            _serviceProvider = serviceProvider;
            _websocketManager = websocketManager;
            _logger = logger;
            _intervalSeconds = configuration.GetValue<int>("TruckStatistics:BroadcastIntervalSeconds", 5);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Truck Statistics Broadcast Service started. Interval: {Interval}s", _intervalSeconds);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_websocketManager.GetConnectionCount() > 0)
                    {
                        // Giống như việc mình DI ITruckStatisticsService vào dưới dạng Scope để sử dụng cho việc lấy CurrentStatistics
                        using var scope = _serviceProvider.CreateScope();
                        var truckStatsService = scope.ServiceProvider.GetRequiredService<ITruckStatisticsService>();
                        // Query Data GetCurrent Statistics
                        var statistics = await truckStatsService.GetCurrentMonthStatisticsAsync();
                        var json = JsonSerializer.Serialize(statistics, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            WriteIndented = false
                        });
                        //BroadCast to All Connection
                        await _websocketManager.BroadCastAsync(json);
                        _logger.LogDebug("Broadcasted truck statistics to {Count} clients. Total trips: {Total}", _websocketManager.GetConnectionCount(), statistics.totalCompletedOrder);
                    }
                    else
                    {
                        _logger.LogTrace("No Clients connected , skipping query");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error broadcasting truck statistics");
                }
                // Chờ 5 giây trước khi query lần tiếp theo
                await Task.Delay(_intervalSeconds * 1000, stoppingToken);
            }
            _logger.LogInformation("Truck Statistics Broadcast Service stopped");
        }
    }
}
