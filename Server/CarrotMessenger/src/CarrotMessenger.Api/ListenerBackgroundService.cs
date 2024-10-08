using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace CarrotMessenger.Api
{
    public class ListenerBackgroundService : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://*:5000/");
            listener.Start();

            Task.Run(async () =>
            {
                while (true)
                {
                    var context = await listener.GetContextAsync();

                    if (context.Request.IsWebSocketRequest)
                    {
                        var username = context.Request.Headers["name"];


                        var wsContext = await context.AcceptWebSocketAsync(null);


                        Console.WriteLine("Client connected " + username);
                        _ = Task.Run(() => HandleConnection(wsContext.WebSocket), cancellationToken);
                        _ = Task.Run(() => SendMessagesToClient(wsContext.WebSocket), cancellationToken);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            });
        }

        static async Task HandleConnection(WebSocket socket)
        {
            byte[] buffer = new byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    Console.WriteLine("Client disconnected");
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message}");
                    string response = "Echo: " + message;
                    byte[] responseBuffer = Encoding.UTF8.GetBytes(response);
                    await socket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true,
                        CancellationToken.None);
                }
            }
        }

        static async Task SendMessagesToClient(WebSocket socket)
        {
            while (socket.State == WebSocketState.Open)
            {
                string message = Console.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
                        CancellationToken.None);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Service Stopped");
            return Task.CompletedTask;
        }
    }
}