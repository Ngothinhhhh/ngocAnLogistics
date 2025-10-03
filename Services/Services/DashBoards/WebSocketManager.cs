using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using WebApplication3.Models;

namespace WebApplication3.Services.Services.DashBoards
{
    public class WebSocketManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new(); // Dictionary thread-safe : nhiều thread có thể truy cập đồng thời mà không bị lỗi
                                                                                   // string key : ID  , WebSocket đối tượng connect : lưu trữ tất cả connection với server ,  1 chuỗi connection sẽ là 1 object socket

        private readonly ConcurrentDictionary<int, List<string>> _userConnections = new();
        private readonly ILogger<WebSocketManager> _logger;    // in ra log của hệ thống , có <WebSocketManager> sẽ in ra theo tên class

        public WebSocketManager(ILogger<WebSocketManager> logger)
        {
            _logger = logger;
        }

        public string AddSocketByUserID(int userID , WebSocket socket)
        {
            // thêm socket và ID mới vào DictionaryConcurrent
            string connectionID = Guid.NewGuid().ToString();
            _sockets.TryAdd(connectionID, socket );

            _userConnections.AddOrUpdate(
                userID ,
                new List<string> { connectionID },
                 (key, list) =>
                 {
                     list.Add(connectionID);    
                     return list;   
                 }
            );
            _logger.LogInformation("User {UserId} connected with Connection {ConnectionId}.", userID, connectionID);
            return connectionID;    
        }

        public async Task RemoveSocketByUserIDAsync(int userID, string connectionID)
        {
            if (_sockets.TryRemove(connectionID, out var socket))
            {
                try
                {
                    // check trạng thái socket
                    if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                    {
                        await socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Connection closed",
                            CancellationToken.None
                        );
                        if (_userConnections.TryGetValue(userID, out List<string> userConnection))
                        {
                            userConnection.Remove(connectionID);
                            if (userConnection.Count == 0)
                            {
                                _userConnections.TryRemove(userID, out _);
                            }
                        }                 
                    }
                }
                catch (WebSocketException ex)
                {
                    _logger.LogWarning(ex, "Error closing socket {ConnectionId} of USERID : {userID}", connectionID , userID);
                }
                finally // cần finnaly để Dispose() luôn luôn đóng tránh Memory Leak
                {
                    socket.Dispose();
                    _logger.LogInformation("Client {userID} disconnected: {ConnectionId}. Total: {Count}", userID , connectionID, _sockets.Count);
                }
            }
        }
        public async Task RemoveSocketByConnectionAsync(string connectionID)
        {
            if (_sockets.TryRemove(connectionID, out var socket))
            {
                try
                {
                    // check trạng thái socket
                    if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                    {
                        await socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Connection closed",
                            CancellationToken.None
                        );
                        foreach (var kvp in _userConnections)
                        {
                            if (kvp.Value.Remove(connectionID))
                            {
                                if (kvp.Value.Count == 0)
                                {
                                    _userConnections.TryRemove(kvp.Key ,out _);
                                }
                                _logger.LogInformation("Remove Socket Successfully {ConnectionId} ", connectionID);
                                break;
                            }
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    _logger.LogWarning(ex, "Error closing socket {ConnectionId} ", connectionID);
                }
                finally // cần finnaly để Dispose() luôn luôn đóng tránh Memory Leak
                {
                    socket.Dispose();
                    _logger.LogInformation("Client disconnected: {ConnectionId}. Total: {Count}" , connectionID, _sockets.Count);
                }
            }
        }

        //BroadCast
        public async Task BroadCastAsync(string message) 
        {
            // BroadCast đến tất cả Client
            var bytes = Encoding.UTF8.GetBytes(message);  //Chuyển string → byte[] (WebSocket chỉ gửi byte)
            var tasks = new List<Task>();   
            foreach (var kvp in _sockets)
            {
                tasks.Add(SendAsync(kvp.Key, kvp.Value, bytes));              
            }
            await Task.WhenAll(tasks);
        }

        // Gửi dữ liệu qua 1 người dùng cụ thể
        public async Task SendToUserAsync(int userID , string message)
        {
            // BroadCast đến tất cả Client
            var bytes = Encoding.UTF8.GetBytes(message);  //Chuyển string → byte[] (WebSocket chỉ gửi byte)
            var tasks = new List<Task>();
            if (_userConnections[userID].Count != 0)
            {
                foreach (var kvp in _userConnections[userID])
                {
                    tasks.Add(SendAsync(kvp, _sockets[kvp], bytes));
                }
                await Task.WhenAll(tasks);
            }
        }


        private async Task SendAsync( string connectionID, WebSocket socket, byte[] bytes)
        {
            try
            {
                await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    endOfMessage :true,
                    CancellationToken.None
                );
            }
            catch(WebSocketException ex)
            {
                _logger.LogWarning(ex, "Failed to send to {ConnectionId}", connectionID);
                await RemoveSocketByConnectionAsync(connectionID);
            }
        } 

        public int GetConnectionCount() => _sockets.Count;
    }
}
