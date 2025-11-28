using System;
using System.Text; 
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Voxta.TCode.Helper {

    public class WsClient 
    {
        bool m_isConnected = false;
        readonly ClientWebSocket webSocket = new();

        public bool IsConnected()
        {
            return m_isConnected;
        }

        public async Task Connect(string address, int port)
        {
            try
            {
                using (webSocket)
                {
                    // Optional: Set headers if needed
                    // webSocket.Options.AddSubProtocol("my-protocol");
                    var uri = $"ws://{address}:{port}/ws";
                    await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

                    // Start receiving messages in a separate task
                    Task receiveTask = ReceiveMessages(webSocket);

                    // Send initial message
                    SendTCode("D1");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Websocket Error: {ex.Message}");
            }
        }

        public void SendTCode(string message)
        {
            Send(message + '\n');
        }

        public async void Disconnect(string message)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            m_isConnected = false;
        }

        async void Send(string message)
        {
            await SendMessage(webSocket, message);
        }

        async Task ReceiveMessages(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[1024];
            StringBuilder builderCache = new();
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message}");
                    builderCache.Append(message);
                    if(message.EndsWith('\n'))
                    {
                        string response = builderCache.ToString();
                        if(response.StartsWith("TCode"))
                        {
                            m_isConnected = true;
                            Console.WriteLine($"TCode connected: {response}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Receive error: {ex.Message}");
                    break;
                }
            }
        }

        static async Task SendMessage(ClientWebSocket webSocket, string message)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                Console.WriteLine("WebSocket is not open!");
                return;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            //Console.WriteLine($"Sent: {message}");
        }
    }

}