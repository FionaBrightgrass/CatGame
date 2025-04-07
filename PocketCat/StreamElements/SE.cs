using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Windows;
using PocketCat.Controls;
using PocketCat.StaticGame;

namespace PocketCat.StreamElements
{
    public class StreamElementsClient
    {
        private MainWindow _mainWindow;
        private Shop ShopWindow;
        private ClientWebSocket webSocket = new ClientWebSocket();
        private CancellationTokenSource cts = new CancellationTokenSource();
        public string jwtTokenPath = Path.Combine(Globals.AppWide.ApplicationData, "001.dat");
        public StreamElementsClient(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            ShopWindow = mainWindow.ShopWindow;
            jwtToken = Encryption.LoadSecureFile(jwtTokenPath);
            Console.WriteLine(jwtToken);
        }

        public async Task ConnectAsync()
        {
            cts.TryReset();
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
                        token = jwtToken,
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
                token = jwtToken,
                token_type = "jwt"
            };

        }

        public void Disconnect()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose(); // Dispose of CancellationTokenSource
            }

            if (webSocket?.State == WebSocketState.Open)
            {
                webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None).Wait();
            }
        }


        private string _jwtToken; // Backing field for the property

        public string jwtToken
        {
            get => _jwtToken;
            set
            {
                if (_jwtToken != value) // Only act if the value changes
                {
                    _jwtToken = value;

                    Console.WriteLine("JWT Token changed. Reconnecting...");

                    Disconnect(); // Safely disconnect

                    // Recreate the WebSocket instance
                    webSocket.Dispose(); // Dispose of the old instance
                    webSocket = new ClientWebSocket(); // Create a new instance
                    cts = new CancellationTokenSource();
                    // Reestablish the connection with the updated token
                    ConnectAsync();
                }
            }
        }


    }
}
