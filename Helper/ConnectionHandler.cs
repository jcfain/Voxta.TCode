
using System.IO.Ports;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text; 
using Voxta.TCode.Model;

namespace Voxta.TCode.Helper
{
    public class ConnectionHandler
    {
        readonly ILogger Logger;
        SerialPort? serial;
        UdpClient? udpClient;
        WsClient? webSocketClient;
        ConnectionType connectionType = ConnectionType.Serial;
        string address = "tcode.local";
        int port = 80;

        public ConnectionHandler(ILogger<Providers.TCodeProvider> logger)
        {
            Logger = logger;
        }
        public bool IsConnected()
        {
            if(connectionType == ConnectionType.Serial) 
                return IsConnected(ConnectionType.Serial);
            if(connectionType == ConnectionType.UDP)
                return IsConnected(ConnectionType.UDP);
            if(connectionType == ConnectionType.WebSocket)
                return IsConnected(ConnectionType.WebSocket);
            return false;
        }
        public bool IsConnected(ConnectionType type)
        {
            if(type == ConnectionType.Serial) 
                return serial?.IsOpen ?? false;
            if(type == ConnectionType.UDP)
                return udpClient?.Client.Connected ?? false;
            if(type == ConnectionType.WebSocket)
                return webSocketClient?.IsConnected() ?? false;
            return false;
        }

        public void Disconnect()
        {
            Disconnect(ConnectionType.Serial);
            Disconnect(ConnectionType.UDP);
            Disconnect(ConnectionType.WebSocket);
        }
        
        public void Disconnect(ConnectionType type)
        {
            if(type == ConnectionType.Serial && IsConnected(ConnectionType.Serial))
            {
                Logger.LogInformation("Serial Disconnect: {port}", address);
                serial?.Close();
            }
            if(type == ConnectionType.UDP &&  IsConnected(ConnectionType.UDP))
            {
                Logger.LogInformation("UDP Disconnect: {address}", address);
                udpClient?.Client.Close();
            }
            if(type == ConnectionType.WebSocket &&  IsConnected(ConnectionType.WebSocket))
            {
                Logger.LogInformation("Websocket Disconnect: {address}",address);
                webSocketClient?.Disconnect();
            }
        }

        public void Connect(ConnectionType connectionType, string address, int port = 80)
        {
            this.address = address;
            this.port = port;
            this.connectionType = connectionType;
            Logger.LogInformation("Connect");
            SetupConnection().Wait();
        }

        public void SendTCode(string tcode)
        {
            //Logger.LogInformation("Sending tcode: {tcode}", tcode);
            if(connectionType == ConnectionType.Serial && IsConnected(ConnectionType.Serial))
            {
                serial?.WriteLine(tcode + "\n");
            }
            else if(connectionType == ConnectionType.UDP && IsConnected(ConnectionType.UDP))
            {
                udpClient?.Send(Encoding.ASCII.GetBytes(tcode +"\n"), tcode.Length +1);
            }
            else if(connectionType == ConnectionType.WebSocket && IsConnected(ConnectionType.WebSocket))
            {
                webSocketClient?.SendTCode(tcode);
            }
        }

        private async Task SetupConnection()
        {
            if (connectionType == ConnectionType.Serial)
            {
                try
                {
                    if(IsConnected(ConnectionType.Serial))
                        return;
                    serial = new SerialPort();
                    if (!SerialPort.GetPortNames().Contains(address))
                    {
                        Logger.LogError("Serial port not found: {port}", address);
                        return;
                    }
                    serial.PortName = address;
                    serial.BaudRate = 115200;
                    serial.Open();
                    serial.DtrEnable = true;
                    serial.ReadTimeout = 10;
                    Logger.LogInformation("Serial connection started on port: {port}", address);
                }
                catch (Exception e)
                {
                    Logger.LogError("Serial connection FAILED port: {port}", address);
                    Logger.LogError("Reason: {reason}", e.Message);
                }
            }
            else if (connectionType == ConnectionType.UDP)
            {
                // Open UDP port
                try
                {
                    if(udpClient == null)
                        udpClient = new UdpClient();
                    udpClient.Connect(address, port);
                    Logger.LogInformation("UDP connection started {address}:{port}", address, port);
                }
                catch (Exception e)
                {
                    Logger.LogError("UDP connection FAILED {address}:{port}", address, port);
                    Logger.LogError("Reason: {reason}", e.Message);
                }
            }
            else if (connectionType == ConnectionType.WebSocket)
            {
                if(IsConnected(ConnectionType.WebSocket))
                    return;
                webSocketClient = new WsClient();
                // Open Websocket port
                try
                {
                    await webSocketClient.Connect(address, port);
                    Logger.LogInformation("Websocket connection started {address}:{port}", address, port);
                }
                catch (Exception e)
                {
                    Logger.LogError("Websocket connection FAILED {address}:{port}", address, port);
                    Logger.LogError("Reason: {reason}", e.Message);
                }
            }
            else
            {
                Logger.LogError("Unknown connection type: {connectiontype}", connectionType);
            }
        }
        
        private void UDPCallback(IAsyncResult result) 
        {
            // Console.WriteLine("UDP callback");
        }
    }
}