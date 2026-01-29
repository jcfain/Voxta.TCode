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
        ClientWebSocket webSocket = new();
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public event EventHandler<ConnectionEventArgs> ConnectedStateChange;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        Task? receiveTask;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public WsClient()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            webSocket.Options.KeepAliveTimeout = TimeSpan.FromSeconds(10);
        }
        public void ConnectionChange(ConnectState state)
        {
            var args = new ConnectionEventArgs
            {
                Type = Model.ConnectionType.WebSocket,
                State = state
            };

            ConnectedStateChange?.Invoke(this, args);
        }

        public bool IsConnected()
        {
            return webSocket.State == WebSocketState.Open ;
        }

        public async Task Connect(string address, int port)
        {
            if(IsConnected())
                return;
            ConnectionChange(ConnectState.Connecting);
            // var uri = "";
            // if(port == 80)
            //     uri = $"ws://{address}/ws";
            //  else
            webSocket = new();
            var uri = $"ws://{address}:{port}/ws";
            try
            {
                // using (webSocket)
                // {
                    // Optional: Set headers if needed
                    // webSocket.Options.AddSubProtocol("my-protocol");
                    await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
                // }

                // Start receiving messages in a separate task
                receiveTask = ReceiveMessages();

                // Send initial message
                SendTCode("D1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WsClient: Websocket connection Error: {ex.Message} {uri}");
                ConnectionChange(ConnectState.Disconnected);
            }
        }

        public void SendTCode(string message)
        {
            Send(message + '\n');
        }

        public async Task Disconnect()
        {
            ConnectionChange(ConnectState.Disconnecting);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }

        async void Send(string message)
        {
            await SendMessage(webSocket, message);
        }

        async Task ReceiveMessages()
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
                    if(message.EndsWith('\n') || (message.StartsWith('{') && message.Contains("TCode")))// TODO: add proper json parsing for websocket
                    {
                        string response = builderCache.ToString();
                        if(response.Contains("TCode"))
                        {
                            Console.WriteLine($"TCode connected: {response}");
                            ConnectionChange(ConnectState.Connected);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Receive error: {ex.Message}");
                    break;
                }
            }
            ConnectionChange(ConnectState.Disconnected);
        }

        static async Task SendMessage(ClientWebSocket webSocket, string message)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                Console.WriteLine("WebSocket was not open when sending the message: '{message}'", message);
                return;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            //Console.WriteLine($"Sent: {message}");
        }
    }

}