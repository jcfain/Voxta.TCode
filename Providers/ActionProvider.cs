using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text; 
using System.IO.Ports;
using Voxta.Model.Shared;
using Voxta.Model.WebsocketMessages.ClientMessages;
using Voxta.Model.WebsocketMessages.ServerMessages;
using Voxta.Providers.Host;
using System.Net.Sockets;
using Voxta.TCode.Model;
using System.Text.RegularExpressions;

namespace Voxta.TCode.Providers;

// This example shows how to create and act on character action inference.
// Note that this is typically not for user commands, another system will be released later
public class ActionProvider: ProviderBase
{
    private readonly IOptions<TCodeOptions> options;
    readonly SerialPort serial;
    readonly UdpClient udpClient;

    private readonly Device device;


    public ActionProvider(
        IRemoteChatSession session,
        ILogger<ActionProvider> logger,
        IOptions<TCodeOptions> options)
        : base(session, logger)
    {
        this.options = options;
        serial = new SerialPort();
        udpClient = new UdpClient();
        device = new Device(options.Value.DeviceType);
        UpdateSettings();
        SetupConnection();
    }

    private void SetupConnection()
    {
        if(!options.Value.UseUDP)
        {
            try
            {
                if(!SerialPort.GetPortNames().Contains(options.Value.SerialPort))
                {
                    Logger.LogError("Serial port not found: {port}", options.Value.SerialPort);
                    return;
                }
                serial.PortName = options.Value.SerialPort;
                serial.BaudRate = 115200;
                serial.Open();
                serial.DtrEnable = true;
                serial.ReadTimeout = 10;
                Logger.LogInformation("Serial connection started on port: {port}", options.Value.SerialPort);
            }
            catch(Exception e)
            {
                Logger.LogError("Serial connection FAILED port: {port}", options.Value.SerialPort);
                Logger.LogError("Reason: {reason}", e.Message);
            }
        } 
        else
        {
            // Open UDP port
            try
            {
                udpClient.Connect(options.Value.UDPAddress, options.Value.UDPPort);
                Logger.LogInformation("UDP connection started {address}:{port}", options.Value.UDPAddress, options.Value.UDPPort);
            }
            catch(Exception e)
            {
                Logger.LogError("UDP connection FAILED {address}:{port}", options.Value.UDPAddress, options.Value.UDPPort);
                Logger.LogError("Reason: {reason}", e.Message);
            }
        }

    }

    private void SendTCode(string tcode)
    {
        // Logger.LogInformation("Sending tcode: {min}", tcode);
        if(!options.Value.UseUDP)
        {
            if(serial.IsOpen)
                serial.WriteLine(tcode + "\n");
        }
        else if(udpClient.Client.Connected)
        {
            udpClient.Send(Encoding.ASCII.GetBytes(tcode +"\n"), tcode.Length +1);
        }
    }

    private void UDPCallback(IAsyncResult result) 
    {
        // Console.WriteLine("UDP callback");
    }

    protected override async Task OnStartAsync()
    {
        await base.OnStartAsync();
        List<FunctionArgumentDefinition> arguments = [];
        
        // Setup other channels for the device
        foreach(var channelKV in device.ChannelsMap)
        {
            var channel = channelKV.Value;
            if(!channel.Enabled)
                continue;
            if(channel.IsSwitch)
            {
                arguments.Add(new FunctionArgumentDefinition
                {
                    Name = channel.Name + "Intensity",
                    Type = FunctionArgumentType.Integer,
                    Required = true,
                    Description = channel.PositionDescription.Length != 0 ? channel.PositionDescription : string.Format("The intensity of the {0}. It can be a number 0-99.", channel.FullName)
                });
                continue;
            }

            arguments.Add(new FunctionArgumentDefinition
            {
                Name = channel.Name + "Range",
                Type = FunctionArgumentType.Integer,
                Required = true,
                Description = channel.RangeDescription.Length != 0 ? channel.RangeDescription : string.Format("The range of the {0}. It can be a number 10-100.", channel.FullName)
            });
            arguments.Add(new FunctionArgumentDefinition
            {
                Name = channel.Name + "Position",
                Type = FunctionArgumentType.Integer,
                Required = true,
                Description = channel.PositionDescription.Length != 0 ? channel.PositionDescription : string.Format("The position of the {0} range. It can be a number 10-90.", channel.FullName)
            });
            arguments.Add(new FunctionArgumentDefinition
            {
                Name = channel.Name + "Speed",
                Type = FunctionArgumentType.Integer,
                Required = true,
                Description = channel.SpeedDescription.Length != 0 ? channel.SpeedDescription : string.Format("The intensity of the {0}. It can be a number 1-10.", channel.FullName)
            });
        }

        var context = new ClientUpdateContextMessage
        {
            SessionId = SessionId,
            ContextKey = "ChannelActions",
            Actions = 
            [
                new()
                {
                    // The LLM will use this name to call the action, use a good action name
                    Name = "stroke",
                    // Layers allow you to run your actions separately from the scene
                    Layer = "_stroker",
                    // Helps the AI understand when and how to use the function
                    Description = "When {{ char }} wants to give pleasure to {{ user }} sexually. This will start stroking the penis with the range and position specified by {{ char }}.",
                    // This text will be prepended to the AI's response
                    Effect = new ActionEffect
                    {
                        Secret = "{{ char }} is stimulating {{ user }} sexually."
                    },
                    // Optional arguments for your action
                    Arguments = [.. arguments]
                },
                new()
                {
                    Name = "stop",
                    Layer = "_stroker",
                    Description = "When {{ user }} wants {{ char }} to stop.",
                    Effect = new ActionEffect
                    {
                        Secret = "{{ char }} has stopped stimulating {{ user }}."
                    },
                    Arguments =
                    [
                        new FunctionArgumentDefinition
                        {
                            Name = "speed",
                            Type = FunctionArgumentType.Integer,
                            Required = true,
                            Description = "It can be a number 0."
                        }
                    ]
                }
            ]
        };

        // Register our action
        Send(context);

        // Act when an action is called
        HandleMessage<ServerActionMessage>(message =>
        {
            // We only care about our layer
            if (message.Layer != "_stroker") return;

                switch (message.Value)
                {
                    
                    case "stroke":
                        HandleChannelUpdates(message);
                        break;

                    default:
                        foreach(var channelKV in device.ChannelsMap)
                        {
                            if(channelKV.Key == ChannelID.Stroke)
                                continue;
                            var channel = channelKV.Value;
                            channel.Target.Mode = "stop";
                            channel.Target.Top = channel.IsSwitch ? 0 : 5000;
                            channel.Target.Bottom = channel.IsSwitch ? 0 : 5000;
                            Logger.LogInformation("Stop");
                            channel.Target.Speed = 0;
                        }
                        //SendTCode("DSTOP");
                        break;
                }
        });

        Task strokingTask = UpdateTCode();
    }

    private void HandleChannelUpdates(ServerActionMessage message)
    {
        // var channelID = ChannelID.Stroke;
        // var channel = device.ChannelsMap[channelID];
        foreach(var channelKV in device.ChannelsMap)
        {
            var channel = channelKV.Value;
            if(!channel.Enabled)
                continue;
            if(channel.IsSwitch)
            {
                var intensityString = message.Arguments?.FirstOrDefault(a => a.Name == channel.Name +"Intensity")?.Value ?? "undefined";
                int intensity = 0;
                if(!int.TryParse(intensityString, out intensity))
                {
                    Logger.LogError("[HandleMessage] Invalid switched intensity: {value}", intensityString);
                }
                else
                {
                    intensity = Math.Clamp(intensity, 0, 99);
                    SendTCode(GetTCode(channelKV.Key, MathExtension.Map(intensity, 0, 99, channel.Min, channel.Max)));
                }
                continue;
            }
            Logger.LogInformation("[HandleMessage] {name} User min: {min}", channel.FullName, channel.Min);
            Logger.LogInformation("[HandleMessage] {name} User max: {max}", channel.FullName, channel.Max);
/*             var min = message.Arguments?.FirstOrDefault(a => a.Name == "rangeMin")?.Value ?? "undefined";
            var max = message.Arguments?.FirstOrDefault(a => a.Name == "rangeMax")?.Value ?? "undefined"; */
            var rangeString = message.Arguments?.FirstOrDefault(a => a.Name == channel.Name +"Range")?.Value ?? "undefined";
            var positionString = message.Arguments?.FirstOrDefault(a => a.Name == channel.Name +"Position")?.Value ?? "undefined";
            Logger.LogInformation("[HandleMessage] {name} rangeString: {min}", channel.FullName, rangeString);
            Logger.LogInformation("[HandleMessage] {name} positionString: {max}", channel.FullName,  positionString);
            var min = 0;
            var max = 9999;
            int range = 0;
            if(!int.TryParse(rangeString, out range))
            {
                Logger.LogError("[HandleMessage] {name} Invalid range: {range}", channel.FullName,  rangeString);
            }
            else
            {
                int position = 5;
                if(!int.TryParse(positionString, out position))
                {
                    Logger.LogError("[HandleMessage] {name} Invalid position: {position}", channel.FullName,  positionString);
                } 
                else
                {
                    if(range == position)
                    {

                    }
                    //var mid = rangeTotal * (rangePercentage/2);
                    var rangeTotal = Math.Clamp(Math.Abs(channel.Max - channel.Min), 0, 9999);
                    Logger.LogInformation("[HandleMessage] {name} rangeTotal: {value}", channel.FullName,  rangeTotal);
                    var rangePercentage = range/100f;
                    Logger.LogInformation("[HandleMessage] {name} rangePercentage: {value}", channel.FullName,  rangePercentage);
                    var positionPercentage = position/100f;
                    Logger.LogInformation("[HandleMessage] {name} positionPercentage: {value}", channel.FullName,  positionPercentage);
                    var rangeLevel = (int)(rangeTotal * positionPercentage);
                    Logger.LogInformation("[HandleMessage] {name} rangeLevel: {value}", channel.FullName,  rangeLevel);
                    var rangeTCodeMiddle = (int)(rangeTotal * rangePercentage)/2;
                    Logger.LogInformation("[HandleMessage] {name} rangeTCodeMiddle: {value}", channel.FullName,  rangeTCodeMiddle);
                    max = Math.Clamp(rangeLevel + rangeTCodeMiddle, channel.Min, channel.Max);
                    min = Math.Clamp(rangeLevel - rangeTCodeMiddle, channel.Min, channel.Max);
                }
            }
            var speedString = message.Arguments?.FirstOrDefault(a => a.Name == channel.Name +"Speed")?.Value ?? "undefined";
            var speed = 5f;
            if (!float.TryParse(speedString, out speed)) 
            {
                Logger.LogError("[HandleMessage] {name} Invalid speed: {value}", channel.FullName, speedString);
            } 
            else
            {
                speed = Math.Clamp(speed, 1f, 10f);
            }

            channel.Target.Mode = "stroke";
            channel.Target.Top = max;
            channel.Target.Bottom = min;
            channel.Target.Speed = speed;
            Logger.LogInformation("{name}, Top: {top}, Bottom: {bottom}, speed: {speed}", channel.FullName,  max, min, speed);
        }
    }

    private async Task UpdateTCode()
    {
        StringBuilder tcode = new();
        while (true)
        {
            foreach(var channelKV in device.ChannelsMap)
            {
                var channel = channelKV.Value;
                if(!channel.Enabled)
                    continue;
                channel.Speed = Converge(channel.Speed, channel.Target.Speed, 0.05f);
                channel.Top = Converge(channel.Top, channel.Target.Top, 20f);
                channel.Bottom = Converge(channel.Bottom, channel.Target.Bottom, 20f);
                channel.Timer += channel.Speed * 1.2f * 0.0094248f; // ie speed of 20 = 3 strokes per second

                if (channel.Timer > 6.2831853f)
                {
                    if (channel.Target.Mode == "stop")
                    {
                        channel.Timer = 6.2831853f;
                        channel.Speed = 0;
                    }
                    else
                    {
                        channel.Timer -= 6.2831853f;
                    }
                }

                float floatPos = (channel.Top + channel.Bottom + 
                    (channel.Top - channel.Bottom) * 
                    Convert.ToSingle(Math.Cos(channel.Timer))) / 2;
                int pos = Convert.ToInt32(floatPos + 0.5f);
                tcode.Append(GetTCode(channelKV.Key, pos));
                // _ = tcode.Append("I100");
                if(!device.ChannelsMap.Last().Equals(channelKV))
                {
                    _ = tcode.Append(' ');
                }
            }
            SendTCode(tcode.ToString());
            tcode.Clear();
            await Task.Delay(10);
        }
    }

    private float Converge(float input, float target, float rate)
    {
        float output;
        if (input >= target)
        {
            if (input - target <= rate) { output = target; }
            else { output = input - rate; }
        }
        else
        {
            if (target - input <= rate) { output = target; }
            else { output = input + rate; }
        }
        return output;
    }

    private string GetTCode(ChannelID channelID, int value)
    {
        var channel = device.ChannelsMap[channelID];
        value = Math.Clamp(value, channel.Min, channel.Max);
/*         string output;
        if (value > 999)
        {
            output = channel.Name + value.ToString();
        }
        else if (value > 99)
        {
            output = channel.Name + "0" + value.ToString();
        }
        else if (value > 9)
        {
            output = channel.Name + "00" + value.ToString();
        }
        else
        {
            output = channel.Name + "000" + value.ToString();
        }
        return output; */
        return channel.Name + value.ToString().PadLeft(4, '0');
    }

    public void UpdateSettings()
    {
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Stroke)) {
            Logger.LogInformation("Setting user stroke min: {min}", options.Value.StrokeMin);
            device.ChannelsMap[ChannelID.Stroke].Min = options.Value.StrokeMin;
            Logger.LogInformation("Setting user stroke max: {max}", options.Value.StrokeMax);
            device.ChannelsMap[ChannelID.Stroke].Max = options.Value.StrokeMax;
            device.ChannelsMap[ChannelID.Stroke].Enabled = options.Value.StrokeEnabled;
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Surge)) {
            Logger.LogInformation("Setting user surge min: {min}", options.Value.SurgeMin);
            device.ChannelsMap[ChannelID.Surge].Min = options.Value.SurgeMin;
            Logger.LogInformation("Setting user surge max: {max}", options.Value.SurgeMax);
            device.ChannelsMap[ChannelID.Surge].Max = options.Value.SurgeMax;
            device.ChannelsMap[ChannelID.Surge].Enabled = options.Value.SurgeEnabled;
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Sway)) {
            Logger.LogInformation("Setting user sway min: {min}", options.Value.SwayMin);
            device.ChannelsMap[ChannelID.Sway].Min = options.Value.SwayMin;
            Logger.LogInformation("Setting user sway max: {max}", options.Value.SwayMax);
            device.ChannelsMap[ChannelID.Sway].Max = options.Value.SwayMax;
            device.ChannelsMap[ChannelID.Sway].Enabled = options.Value.SwayEnabled;
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Twist)) {
            Logger.LogInformation("Setting user twist min: {min}", options.Value.TwistMin);
            device.ChannelsMap[ChannelID.Twist].Min = options.Value.TwistMin;
            Logger.LogInformation("Setting user twist max: {max}", options.Value.TwistMax);
            device.ChannelsMap[ChannelID.Twist].Max = options.Value.TwistMax;
            device.ChannelsMap[ChannelID.Twist].Enabled = options.Value.TwistEnabled;
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Roll)) {
            Logger.LogInformation("Setting user roll min: {min}", options.Value.RollMin);
            device.ChannelsMap[ChannelID.Roll].Min = options.Value.RollMin;
            Logger.LogInformation("Setting user roll max: {max}", options.Value.RollMax);
            device.ChannelsMap[ChannelID.Roll].Max = options.Value.RollMax;
            device.ChannelsMap[ChannelID.Roll].Enabled = options.Value.RollEnabled;
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Pitch)) {
            Logger.LogInformation("Setting user pitch min: {min}", options.Value.PitchMin);
            device.ChannelsMap[ChannelID.Pitch].Min = options.Value.PitchMin;
            Logger.LogInformation("Setting user pitch max: {max}", options.Value.PitchMax);
            device.ChannelsMap[ChannelID.Pitch].Max = options.Value.PitchMax;
            device.ChannelsMap[ChannelID.Pitch].Enabled = options.Value.PitchEnabled;
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.SuckLevel)) {
            device.ChannelsMap[ChannelID.SuckLevel].Enabled = options.Value.SuckEnabled;
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Vibe1)) {
            device.ChannelsMap[ChannelID.Vibe1].Enabled = options.Value.Vibe1Enabled;
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Vibe2)) {
            device.ChannelsMap[ChannelID.Vibe2].Enabled = options.Value.Vibe2Enabled;
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Lube)) {
            device.ChannelsMap[ChannelID.Lube].Enabled = options.Value.LubeEnabled;
        }
    }
}

