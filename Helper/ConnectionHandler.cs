
using System.IO.Ports;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text; 
using Voxta.TCode.Model;

namespace Voxta.TCode.Helper
{
    public enum ConnectState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting,
    }
    public class ConnectionEventArgs : EventArgs
    {
        public ConnectionType Type { get; set; }
        public ConnectState State { get; set; }
    }
    public class ConnectionHandler
    {
        readonly ILogger Logger;
        SerialPort? serial;
        UdpClient? udpClient;
        WsClient? webSocketClient;
        ConnectionType connectionType = ConnectionType.Serial;
        public event EventHandler<ConnectionEventArgs> ConnectedStateChange;
        ConnectState currentState = ConnectState.Disconnected;
        string address = "tcode.local";
        int port = 80;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ConnectionHandler(ILogger<Providers.TCodeProvider> logger)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            Logger = logger;
            //ConnectedStateChange += OnConnectionChange; TODO: figure out this warning
        }

        public void ConnectionChange(ConnectionType type, ConnectState state)
        {
            var args = new ConnectionEventArgs
            {
                Type = type,
                State = state
            };
            currentState = state;

            ConnectedStateChange?.Invoke(this, args);
        }
        
        public bool IsConnected()
        {
            
            //Logger.LogInformation("ConnectionHandler: IsConnected.");
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
            if(currentState != ConnectState.Connected)
                return;
            ConnectionChange(type, ConnectState.Disconnecting);
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
            ConnectionChange(type, ConnectState.Disconnected);
        }

        public async Task Connect(ConnectionType connectionType, string address, int port = 80)
        {
            if(currentState != ConnectState.Disconnected)
                return;
            if(IsConnected(connectionType))
            {
                Logger.LogInformation("Connect called on {type} when already connected.", connectionType);
                return;
            }
            this.address = address;
            this.port = port;
            this.connectionType = connectionType;
            Logger.LogInformation("Connect");
            _ = SetupConnection();
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
                    ConnectionChange(connectionType, ConnectState.Connecting);
                    if(serial == null)
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
                    if(IsConnected(ConnectionType.Serial)) 
                    {
                        Logger.LogInformation("Serial connection started on port: {port}", address);
                        ConnectionChange(connectionType, ConnectState.Connected);
                        _ = MonitorSerial();
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError("Serial connection FAILED port: {port}", address);
                    Logger.LogError("Reason: {reason}", e.Message);
                    ConnectionChange(connectionType, ConnectState.Disconnected);
                }
            }
            else if (connectionType == ConnectionType.UDP)
            {
                // Open UDP port
                try
                {
                    ConnectionChange(connectionType, ConnectState.Connecting);
                    if(udpClient == null)
                        udpClient = new UdpClient();
                    udpClient.Connect(address, port);
                    if(IsConnected(ConnectionType.UDP)) 
                    {
                        Logger.LogInformation("UDP connection started {address}:{port}", address, port);
                        ConnectionChange(connectionType, ConnectState.Connected);
                        _ = MonitorUdp();
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError("UDP connection FAILED {address}:{port}", address, port);
                    Logger.LogError("Reason: {reason}", e.Message);
                    ConnectionChange(connectionType, ConnectState.Disconnected);
                }
            }
            else if (connectionType == ConnectionType.WebSocket)
            {
                if(IsConnected(ConnectionType.WebSocket))
                    return;
                if(webSocketClient == null)
                    webSocketClient = new WsClient();
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                webSocketClient.ConnectedStateChange += OnConnectionChange;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                              // Open Websocket port
                try
                {
                    await webSocketClient.Connect(address, port);
                    // if(IsConnected(ConnectionType.WebSocket))
                    // {
                    //     Logger.LogInformation("Websocket connection started {address}:{port}", address, port);
                    // }
                }
                catch (Exception e)
                {
                    Logger.LogError("Websocket connection FAILED {address}:{port}", address, port);
                    Logger.LogError("Reason: {reason}", e.Message);
                    ConnectionChange(connectionType, ConnectState.Disconnected);
                }
            }
            else
            {
                Logger.LogError("Unknown connection type: {connectiontype}", connectionType);
                ConnectionChange(connectionType, ConnectState.Disconnected);
            }
        }
        
        private void UDPCallback(IAsyncResult result) 
        {
            // Console.WriteLine("UDP callback");
        }
        private void OnConnectionChange(object sender, ConnectionEventArgs e)
        {
            //Logger.LogInformation("Connection for {type} changeed to: {state}", e.Type, e.State);
            ConnectionChange(e.Type, e.State);
        }

        private async Task MonitorSerial()
        {
            while (IsConnected(ConnectionType.Serial))
            {
                await Task.Delay(1000);
            }
            ConnectionChange(ConnectionType.Serial, ConnectState.Disconnected);
        }
        private async Task MonitorUdp()
        {
            while (IsConnected(ConnectionType.UDP))
            {
                await Task.Delay(1000);
            }
            ConnectionChange(ConnectionType.UDP, ConnectState.Disconnected);
        }
    }
}