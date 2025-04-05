using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PocketCat.Controls;

namespace PocketCat.StreamElements
{
    public class StreamElementsClient
    {
        private MainWindow _mainWindow;
        private Shop ShopWindow;
        private const string StreamElementsWebSocketUrl = "wss://astro.streamelements.com";
        private const string JwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJjaXRhZGVsIiwiZXhwIjoxNzUzMzg4MDU5LCJqdGkiOiI3NmNiNTFmNy1hZTU2LTQ1YjItYTA3Zi01YTFjYTJlNmI0MmQiLCJjaGFubmVsIjoiNjZmNzk0OGU5MWM5NzE4NmExZjU4ODE0Iiwicm9sZSI6Im93bmVyIiwiYXV0aFRva2VuIjoiTlJ6dVpEdFlONXlEOHpuNjdHTXBMazdjcnptSVZGVV9ZM21oNE50bGRSdzNOa2szIiwidXNlciI6IjY2Zjc5NDhlOTFjOTcxODZhMWY1ODgxMyIsInVzZXJfaWQiOiJjYWZkZmMxZC05NzAzLTQ4MmEtOTkzNS04MjdmOGQxYzcwZjYiLCJ1c2VyX3JvbGUiOiJjcmVhdG9yIiwicHJvdmlkZXIiOiJ0d2l0Y2giLCJwcm92aWRlcl9pZCI6IjkyNjkwNzg1OSIsImNoYW5uZWxfaWQiOiJjM2MzMGUyNy0xMWE3LTQxZjQtYTMyOS1iN2ZiOGRmMGRmOTYiLCJjcmVhdG9yX2lkIjoiNzZiNTk1YTEtNTdlNC00OWRiLWEzYzAtMTk1NDA1ZDMzM2E3In0.VUCIpRqPwaZKx3dlTZTK3THNcjXqFo - erxd3EyVZyto";
        private ClientWebSocket webSocket;
        private CancellationTokenSource cts;

        public StreamElementsClient(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            ShopWindow = mainWindow.ShopWindow;
        }

        public async Task ConnectAsync()
        {
            webSocket = new ClientWebSocket();
            cts = new CancellationTokenSource();

            try
            {
                await webSocket.ConnectAsync(new Uri("wss://astro.streamelements.com"), cts.Token);
                Console.WriteLine("Connected to StreamElements Astro");

                // Subscribe to a topic
                var chatMessage = new
                {
                    type = "subscribe",
                    nonce = Guid.NewGuid().ToString(),
                    data = new
                    {
                        topic = "channel.chatbot.counters",
                        token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJjaXRhZGVsIiwiZXhwIjoxNzUzMzg4MDU5LCJqdGkiOiI3NmNiNTFmNy1hZTU2LTQ1YjItYTA3Zi01YTFjYTJlNmI0MmQiLCJjaGFubmVsIjoiNjZmNzk0OGU5MWM5NzE4NmExZjU4ODE0Iiwicm9sZSI6Im93bmVyIiwiYXV0aFRva2VuIjoiTlJ6dVpEdFlONXlEOHpuNjdHTXBMazdjcnptSVZGVV9ZM21oNE50bGRSdzNOa2szIiwidXNlciI6IjY2Zjc5NDhlOTFjOTcxODZhMWY1ODgxMyIsInVzZXJfaWQiOiJjYWZkZmMxZC05NzAzLTQ4MmEtOTkzNS04MjdmOGQxYzcwZjYiLCJ1c2VyX3JvbGUiOiJjcmVhdG9yIiwicHJvdmlkZXIiOiJ0d2l0Y2giLCJwcm92aWRlcl9pZCI6IjkyNjkwNzg1OSIsImNoYW5uZWxfaWQiOiJjM2MzMGUyNy0xMWE3LTQxZjQtYTMyOS1iN2ZiOGRmMGRmOTYiLCJjcmVhdG9yX2lkIjoiNzZiNTk1YTEtNTdlNC00OWRiLWEzYzAtMTk1NDA1ZDMzM2E3In0.VUCIpRqPwaZKx3dlTZTK3THNcjXqFo-erxd3EyVZyto",
                        token_type = "jwt"
                    }
                };

                await SendMessageAsync(chatMessage);

                // Start receiving messages
                await ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting: {ex.Message}");
            }
        }

        private async Task SendMessageAsync(object message)
        {
            string json = JsonSerializer.Serialize(message);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cts.Token);
        }

        private async Task ReceiveMessagesAsync()
        {
            byte[] buffer = new byte[4096];

            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                    Console.WriteLine(result.ToString());
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Message received: {json}");

                        using (JsonDocument doc = JsonDocument.Parse(json))
                        {
                            JsonElement root = doc.RootElement;
                            string messageType = root.GetProperty("type").GetString();

                            if (messageType == "message") // Chat event
                            {
                                string topic = root.GetProperty("topic").GetString();
                                if (topic == "channel.chatbot.counters") // Ensure it's a chat message
                                {
                                    JsonElement data = root.GetProperty("data");
                                    string text = data.GetProperty("counter").GetString(); // Chat message text

                                    // Check for !PetTip command
                                    if (text.Contains("Payout", StringComparison.OrdinalIgnoreCase))
                                    {
                                        _mainWindow.TryAcceptTip();
                                        Console.WriteLine($"Player's new balance: {ShopWindow.ViewModel.Money.Amount}");
                                    }
                                    else
                                    {
                                        Console.WriteLine(text);
                                        _mainWindow.TryAcceptFood(text);
                                        _mainWindow.TryAcceptToy(text);
                                    }
                                }
                            }
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by server", cts.Token);
                        Console.WriteLine("Connection closed");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving message: {ex.Message}");
                    break;
                }
            }
        }
        private async Task SendBotMessage(string responseMessage)
        {
            var botMessage = new
            {
                type = "chat", // Chat message type
                data = new { message = responseMessage }
            };

            await SendMessageAsync(botMessage);
            Console.WriteLine($"Bot message sent: {responseMessage}");
        }


        public async Task UnsubscribeAsync(string topic, string room = null)
        {
            var data = new
            {
                topic = topic,
                token = "YOUR_JWT_TOKEN",
                token_type = "jwt"
            };

        }

        public void Disconnect()
        {
            if (webSocket.State == WebSocketState.Open)
            {
                webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", cts.Token).Wait();
            }

            cts.Cancel();
        }
    }
}
