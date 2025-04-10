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

    private readonly Dictionary<ChannelID, Channel> ChannelsMap = new()
    {
        { ChannelID.Stroke, new Channel(ChannelName.Stroke, "Stroke")},
        { ChannelID.Surge, new Channel(ChannelName.Surge, "Surge")},
        { ChannelID.Sway, new Channel(ChannelName.Sway, "Sway")},
        { ChannelID.Twist, new Channel(ChannelName.Twist, "Twist")},
        { ChannelID.Roll, new Channel(ChannelName.Roll, "Roll")},
        { ChannelID.Pitch, new Channel(ChannelName.Pitch, "Pitch")}
    };


    public ActionProvider(
        IRemoteChatSession session,
        ILogger<ActionProvider> logger,
        IOptions<TCodeOptions> options)
        : base(session, logger)
    {
        this.options = options;
        serial = new SerialPort();
        udpClient = new UdpClient();
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

        Logger.LogInformation("[OnStartAsync] User stroke min: {min}", ChannelsMap[ChannelID.Stroke].Min);
        Logger.LogInformation("[OnStartAsync] User stroke max: {max}", ChannelsMap[ChannelID.Stroke].Max);
        // Register our action
        Send(new ClientUpdateContextMessage
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
                    Arguments =
                    [
                 /*        new FunctionArgumentDefinition
                        {
                            // The name sent to the LLM
                            Name = "rangeMin",
                            // The type of argument
                            Type = FunctionArgumentType.Integer,
                            // Whether to access failure to generate this argument
                            Required = true,
                            // Explanation for the LLM
                            Description = string.Format("The lower range of the strokes. It can be a number {0}-{1} but must be less than the argument rangeMax.", ChannelsMap[ChannelID.Stroke].Min, ChannelsMap[ChannelID.Stroke].Max)
                        }, 
                        new FunctionArgumentDefinition
                        {
                            Name = "rangeMax",
                            Type = FunctionArgumentType.Integer,
                            Required = true,
                            Description = string.Format("The upper range of the strokes. It can be a number {0}-{1} but must be greater than the argument rangeMin.", ChannelsMap[ChannelID.Stroke].Min, ChannelsMap[ChannelID.Stroke].Max)
                        },  */
                        new FunctionArgumentDefinition
                        {
                            Name = "range",
                            Type = FunctionArgumentType.Integer,
                            Required = true,
                            Description = "The range of the stroke. It can be a number 10-100."
                        }, 
                        new FunctionArgumentDefinition
                        {
                            Name = "position",
                            Type = FunctionArgumentType.Integer,
                            Required = true,
                            Description = "The position of the stroke range. Where 90 is the head or tip of the cock, 10 is the base of the cock and 50 is the middle of the cock. It can be a number 10-90."
                        }, 
                        new FunctionArgumentDefinition
                        {
                            Name = "speed",
                            Type = FunctionArgumentType.Integer,
                            Required = true,
                            Description = "The intensity of the strokes. It can be a number 1-10."
                        }
                    ]
                },
/*                 
                new()
                {
                    // The LLM will use this name to call the action, use a good action name
                    Name = "long_range",
                    // Layers allow you to run your actions separately from the scene
                    Layer = "_stroker",
                    // Helps the AI understand when and how to use the function
                    Description = "When {{ char }} wants to give pleasure to {{ user }} sexually.",
                    // This text will be prepended to the AI's response
                    Effect = new ActionEffect
                    {
                        Secret = "{{ char }} is stimulating {{ user }} sexually."
                    },
                    // Optional arguments for your action
                    Arguments =
                    [
                        new FunctionArgumentDefinition
                        {
                            // The name sent to the LLM
                            Name = "speed",
                            // The type of argument
                            Type = FunctionArgumentType.Integer,
                            // Whether to access failure to generate this argument
                            Required = true,
                            // Explanation for the LLM
                            Description = "The intensity of the long strokes. It can be a number 1-10."
                        }
                    ]
                },
                new()
                {
                    Name = "min_half",
                    Layer = "_stroker",
                    Description = "When {{ char }} wants to avoid over stimulation to {{ user }} by stroking the lower half of the penis",
                    Effect = new ActionEffect
                    {
                        Secret = "{{ char }} is giving {{ user }} intense sexual stimulation."
                    },
                    Arguments =
                    [
                        new FunctionArgumentDefinition
                        {
                            Name = "speed",
                            Type = FunctionArgumentType.Integer,
                            Required = true,
                            Description = "The intensity of the lower short strokes. It can be a number 1-10."
                        }
                    ]
                },
                new()
                {
                    Name = "max_half",
                    Layer = "_stroker",
                    Description = "When {{ char }} wants to give intense pleasure to {{ user }} sexually or go really fast.",
                    Effect = new ActionEffect
                    {
                        Secret = "{{ char }} is giving {{ user }} intense sexual stimulation."
                    },
                    Arguments =
                    [
                        new FunctionArgumentDefinition
                        {
                            Name = "speed",
                            Type = FunctionArgumentType.Integer,
                            Required = true,
                            Description = "The intensity of the upper short strokes. It can be a number 1-10."
                        }
                    ]
                },
                new()
                {
                    Name = "max_thirty_percent",
                    Layer = "_stroker",
                    Description = "When {{ char }} wants to give gentle pleasure to {{ user }} sexually or go slow.",
                    Effect = new ActionEffect
                    {
                        Secret = "{{ char }} is giving {{ user }} gentle sexual stimulation."
                    },
                    Arguments =
                    [
                        new FunctionArgumentDefinition
                        {
                            Name = "speed",
                            Type = FunctionArgumentType.Integer,
                            Required = true,
                            Description = "The intensity of the teasing strokes. It can be a number 1-10."
                        }
                    ]
                },
                new()
                {
                    Name = "slow_long_range",
                    Layer = "_stroker",
                    Description = "When {{ char }} wants to tease {{ user }} by denying stimulation.",
                    Effect = new ActionEffect
                    {
                        Secret = "{{ char }} has slowed stimulating {{ user }}."
                    },
                    Arguments =
                    [
                        new FunctionArgumentDefinition
                        {
                            Name = "speed",
                            Type = FunctionArgumentType.Integer,
                            Required = true,
                            Description = "The strength of the denial. It can be a number 1-10."
                        }
                    ]
                }, 
        */
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
        });

        // Act when an action is called
        HandleMessage<ServerActionMessage>(message =>
        {
            // We only care about our layer
            if (message.Layer != "_stroker") return;
            var channelID = ChannelID.Stroke;
            var channel = ChannelsMap[channelID];
            Logger.LogInformation("[HandleMessage] User min: {min}", channel.Min);
            Logger.LogInformation("[HandleMessage] User max: {max}", channel.Max);
/*             var min = message.Arguments?.FirstOrDefault(a => a.Name == "rangeMin")?.Value ?? "undefined";
            var max = message.Arguments?.FirstOrDefault(a => a.Name == "rangeMax")?.Value ?? "undefined"; */
            var rangeString = message.Arguments?.FirstOrDefault(a => a.Name == "range")?.Value ?? "undefined";
            var positionString = message.Arguments?.FirstOrDefault(a => a.Name == "position")?.Value ?? "undefined";
            Logger.LogInformation("[HandleMessage] rangeString: {min}", rangeString);
            Logger.LogInformation("[HandleMessage] positionString: {max}", positionString);
            var min = 0;
            var max = 9999;
            int range = 0;
            if(!int.TryParse(rangeString, out range))
            {
                Logger.LogError("[HandleMessage] Invalid range: {range}", rangeString);
            }
            else
            {
                int position = 5;
                if(!int.TryParse(positionString, out position))
                {
                    Logger.LogError("[HandleMessage] Invalid position: {position}", positionString);
                } 
                else
                {
                    if(range == position)
                    {

                    }
                    //var mid = rangeTotal * (rangePercentage/2);
                    var rangeTotal = Math.Clamp(Math.Abs(channel.Max - channel.Min), 0, 9999);
                    Logger.LogInformation("[HandleMessage] rangeTotal: {value}", rangeTotal);
                    var rangePercentage = range/100f;
                    Logger.LogInformation("[HandleMessage] rangePercentage: {value}", rangePercentage);
                    var positionPercentage = position/100f;
                    Logger.LogInformation("[HandleMessage] positionPercentage: {value}", positionPercentage);
                    var rangeLevel = (int)(rangeTotal * positionPercentage);
                    Logger.LogInformation("[HandleMessage] rangeLevel: {value}", rangeLevel);
                    var rangeTCodeMiddle = (int)(rangeTotal * rangePercentage)/2;
                    Logger.LogInformation("[HandleMessage] rangeTCodeMiddle: {value}", rangeTCodeMiddle);
                    max = Math.Clamp(rangeLevel + rangeTCodeMiddle, channel.Min, channel.Max);
                    min = Math.Clamp(rangeLevel - rangeTCodeMiddle, channel.Min, channel.Max);
                }
            }
            var speedString = message.Arguments?.FirstOrDefault(a => a.Name == "speed")?.Value ?? "undefined";
            var speed = 5f;
            if (!float.TryParse(speedString, out speed)) 
            {
                Logger.LogError("[HandleMessage] Invalid speed: {value}", speedString);
            } 
            else
            {
                speed = Math.Clamp(speed, 1f, 10f);
            }

            switch (message.Value)
            {
                
                case "stroke":
                    channel.Target.Mode = "stroke";
                    channel.Target.Top = max;
                    channel.Target.Bottom = min;
                    Logger.LogInformation("Stroke, Top: {top}, Bottom: {bottom}, speed: {speed}", max, min, speed);
                    channel.Target.Speed = speed;
                    break;

                case "long_range":
                    channel.Target.Mode = "long_range";
                    channel.Target.Top = channel.Max;
                    channel.Target.Bottom = channel.Min;
                    Logger.LogInformation("Long strokes, speed: {speed}", speed);
                    channel.Target.Speed = speed;
                    break;

                case "min_half":
                    channel.Target.Mode = "min_half";
                    channel.Target.Top = (channel.Max - channel.Min) / 2;
                    channel.Target.Bottom = channel.Min;
                    Logger.LogInformation("Min half, speed {speed}", speed);
                    channel.Target.Speed = 5f + 1.5f * speed;
                    break;

                case "max_half":
                    channel.Target.Mode = "max_half";
                    channel.Target.Top = channel.Max;
                    channel.Target.Bottom = (channel.Max - channel.Min) / 2;
                    Logger.LogInformation("Max half, speed {speed}", speed);
                    channel.Target.Speed = 5f + 1.5f * speed;
                    break;

                case "max_thirty_percent":
                    channel.Target.Mode = "max_thirty_percent";
                    channel.Target.Top = channel.Max;
                    channel.Target.Bottom = channel.Max - (int)Math.Round(channel.Max * 0.30, 0);
                    Logger.LogInformation("Max thirty percent, speed: {speed}", speed);
                    channel.Target.Speed = 0.6f * speed;
                    break;

                case "slow_long_range":
                    channel.Target.Mode = "slow";
                    channel.Target.Top = channel.Max;
                    channel.Target.Bottom = channel.Min;
                    Logger.LogInformation("Slow, Speed: {speed}", speed);
                    channel.Target.Speed = speed;
                    break;

                default:
                    channel.Target.Mode = "stop";
                    channel.Target.Top = channel.Max;
                    channel.Target.Bottom = channel.Min;
                    Logger.LogInformation("Stop: {speed}", speed);
                    channel.Target.Speed = 0;
                    break;
            }

        });

        Task strokingTask = UpdateTCode();
    }

    private async Task UpdateTCode()
    {
        var channelID = ChannelID.Stroke;
        var channel = ChannelsMap[channelID];
        while (true)
        {
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

            SendTCode(GetTCode(channelID, pos));
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
        var channel = ChannelsMap[channelID];
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
        Logger.LogInformation("Setting user stroke min: {min}", options.Value.StrokeMin);
        ChannelsMap[ChannelID.Stroke].Min = options.Value.StrokeMin;
        Logger.LogInformation("Setting user stroke max: {max}", options.Value.StrokeMax);
        ChannelsMap[ChannelID.Stroke].Max = options.Value.StrokeMax;
    }
}

