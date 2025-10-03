using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using WebApplication3.DTOs;
using WebApplication3.DTOs.CustomerDTOs;
using WebApplication3.DTOs.UserDTO;
using WebApplication3.Helpers;
using WebApplication3.Middlewares;
using WebApplication3.Models;
using WebApplication3.Services.Services.DashBoards;
using WebSocketManager = WebApplication3.Services.Services.DashBoards.WebSocketManager;

namespace WebApplication3.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoardController : ControllerBase
    {
        private readonly WebSocketManager _webSocketManager;
        private readonly ILogger<DashBoardController> _logger;
        public DashBoardController(WebSocketManager webSocketManager, ILogger<DashBoardController> logger)
        {
            _webSocketManager = webSocketManager;
            _logger = logger;
        }

        /// WebSocket endpoint cho truck statistics dashboard
        /// URL: ws://localhost:5000/api/websocket/truck-statistics
        //[JwtAuthMiddleware]
        [HttpGet("truck-statistics")]
        public async Task TruckStatistics()
        {
            //var dataUser = HttpContext.Items["User"] as UserClaimsDTO;
            //var userID = dataUser.UserID;
            var userID = 1;
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsync("WebSocket Connection required");
            }
            WebSocket webSocket = null;
            string connectionId = null;
            try
            {
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                connectionId = _webSocketManager.AddSocketByUserID(userID, webSocket);
                _logger.LogInformation(
                    "Truck statistics WebSocket connected: {ConnectionId}",
                    connectionId
                );
                // Giữ connection sống - chỉ lắng nghe
                var buffer = new byte[1024 * 4];

                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        HttpContext.RequestAborted // ✅ token này bị cancel khi app shutdown
                    );
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation(
                            "Client requested close: {ConnectionId}",
                            connectionId
                        );
                        break;
                    }
                }

            }
            catch (WebSocketException wsEx)
            {
                _logger.LogWarning(wsEx, "WebSocket error for {ConnectionId}", connectionId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation cancelled for {ConnectionId}", connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error for {ConnectionId}", connectionId);
            }
            finally
            {
                if (connectionId != null)
                {
                    await _webSocketManager.RemoveSocketByUserIDAsync(userID, connectionId);
                }
            }
        }

        [HttpGet("CurrentConnectionCount")]
        public async Task<IActionResult> GetConnectionCount()
        {
            return Ok(new ApiResponse<int>
            {
                Data = _webSocketManager.GetConnectionCount(),
                Message = "",
                statusCode = 200
            });
        }

    }


}
