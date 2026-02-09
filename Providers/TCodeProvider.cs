using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text; 
using Voxta.Model.Shared;
using Voxta.Model.WebsocketMessages.ClientMessages;
using Voxta.Model.WebsocketMessages.ServerMessages;
using Voxta.Providers.Host;
using Voxta.TCode.Model;
using Voxta.TCode.Helper;
using System.Security.Cryptography.X509Certificates;

namespace Voxta.TCode.Providers;
// This controls a tcode device
//[UsedImplicitly]
public class TCodeProvider : ProviderBase
{
    private readonly IOptions<TCodeOptions> options;

    private readonly Device device;
    private Task? updateTCodeTask;
    private readonly ConnectionHandler connectionHandler;
    private UserDevice m_selectedDevice;

    public TCodeProvider(
        IRemoteChatSession session,
        ILogger<TCodeProvider> logger,
        IOptions<TCodeOptions> options)
        : base(session, logger)
    {
        this.options = options;
        var selected = GetUserDevice(options.Value.SelectedDevice);
        if(selected == null)
        {
            throw new Exception("No devices in settings. Exiting...");
        }
        m_selectedDevice = selected;
        connectionHandler = new(logger);
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
        connectionHandler.ConnectedStateChange += OnConnectionChange;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
        device = new Device(m_selectedDevice.DeviceType);
        InitSettings(device, m_selectedDevice);
        connectionHandler.Init(m_selectedDevice.ConnectionType);
        updateTCodeTask = UpdateTCode();
    }

    private async Task Connect()
    {
        if(m_selectedDevice.ConnectionType == ConnectionType.Serial && !connectionHandler.IsConnected(ConnectionType.Serial)) 
        {
            await connectionHandler.Connect(m_selectedDevice.ConnectionType, m_selectedDevice.SerialPort);
        }
        else if(m_selectedDevice.ConnectionType == ConnectionType.UDP || m_selectedDevice.ConnectionType == ConnectionType.WebSocket)
        {
            await connectionHandler.Connect(m_selectedDevice.ConnectionType, m_selectedDevice.NetworkAddress, m_selectedDevice.NetworkPort);
        }
        await Task.Delay(10);
    }

    private async Task Disconnect()
    {
        await connectionHandler.Disconnect();
    }

    private async Task Reconnect()
    {
        await Disconnect();
        await Connect();
    }

    private async Task ChangeConnection(string name)
    {
        await Disconnect();
        var selectedDevice = GetUserDevice(name);
        if(selectedDevice == null)
        {
            return;
        }
        m_selectedDevice = selectedDevice;
        InitSettings(device, m_selectedDevice);
        await Connect();
    }

    public void OnConnectionChange(object sender, ConnectionEventArgs e)
    {
        Logger.LogInformation("Connection for {type} changed to: {state}", e.Type, e.State);
        if(e.State == ConnectState.Connected || e.State == ConnectState.Disconnected)
            Send(GetContext());
    }

    protected override void OnMessage(ServerChatSessionMessage message)
    {
        //SendWhenFree(new TCodeApp());
    }
    protected override async Task OnStartAsync()
    {
        await base.OnStartAsync();
        Logger.LogInformation("OnStartAsync");
        //_ = Connect(); Wait for the user is ready to connect
       
        // Register our action
        //Send(GetContext());

        // Act when an action is called
        Logger.LogInformation("OnStartAsync : HandleMessage");
        HandleMessage<ServerActionMessage>(message =>
        {
            // We only care about our layer
            if (message.Layer != "_stroker") return;

            if(message.Value.StartsWith($"{DeviceActions.Connect}: "))
            {
                var name = message.Value.Replace($"{DeviceActions.Connect}: ", "");
                _ = ChangeConnection(name);
                return;
            }

            switch (message.Value)
            {
                
                case DeviceActions.Stroke:
                    Logger.LogInformation("HandleMessage: Stroke");
                    HandleChannelUpdates(message);
                    break;
                case DeviceActions.Stop:
                    foreach(var channelKV in device.ChannelsMap)
                    {
                        var channel = channelKV.Value;
                        StopChannel(ref channel);
                    }
                    Logger.LogInformation("HandleMessage: Stop");
                    break;
                case DeviceActions.Connect:
                    Logger.LogInformation("HandleMessage: Connect");
                    _ = Connect();
                    break;
                case DeviceActions.Disconnect:
                    Logger.LogInformation("HandleMessage: Disconnect");
                    _ = Disconnect();
                    break;
                case DeviceActions.Wait:
                    Logger.LogInformation("HandleMessage: Wait");
                    break;
                default:
                    Logger.LogInformation("HandleMessage: Unknown command {command}", message.Value);
                    break;
            }
        });
    }

    private ClientUpdateContextMessage GetContext()
    { 
        //Logger.LogInformation("GetContext");
        ClientUpdateContextMessage context;
        if(connectionHandler.IsConnected())
        {
            Logger.LogInformation("GetContext: connected");
            var functionDescription = "When {{ char }} wants to physically interact with {{ user }} in a sexual manner.";
            var arguments = BuildChannelArguments(ref functionDescription);
            context = new ClientUpdateContextMessage
            {
                SessionId = SessionId,
                ContextKey = "device",
                Contexts = [new() { Text = "{{ user }}'s stroker device is connected." }],
                Actions = 
                [
                    new()
                    {
                        // The LLM will use this name to call the action, use a good action name
                        Name = DeviceActions.Stroke,
                        // Layers allow you to run your actions separately from the scene
                        Layer = "_stroker",
                        // Helps the AI understand when and how to use the function
                        Description = functionDescription,
                        // This text will be prepended to the AI's response
                        Effect = new ActionEffect
                        {
                            Secret = "{{ char }} is physically stimulating {{ user }} in a sexual manner."
                        },
                        Timing = FunctionTiming.AfterAssistantMessage,
                        // Optional arguments for your action
                        Arguments = [.. arguments]
                    },
                    new()
                    {
                        Name = DeviceActions.Stop,
                        Layer = "_stroker",
                        Description = "When {{ user }} or {{ char }} wants to stop all sexual stimulation.",
                        Effect = new ActionEffect
                        {
                            Secret = "{{ char }} has stopped physically stimulating {{ user }}."
                        },
                        Timing = FunctionTiming.AfterAssistantMessage,
                        FinalLayer = false,
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
                    },
                    new()
                    {
                        Name = DeviceActions.Wait,
                        Layer = "_stroker",
                        Description = "When {{ char }} is waiting to keep the device in the current state.",
                        Effect = new ActionEffect
                        {
                            Secret = $"{{{{ char }}}} is holding the current device state."
                        },
                        Timing = FunctionTiming.AfterAssistantMessage,
                        FinalLayer = false,
                        Arguments = []
                    },
                    new()
                    {
                        Name = DeviceActions.Disconnect,
                        Layer = "_stroker",
                        Description = "When {{ user }} wants to disconnect the device. DO NOT EXECUTE unless the user asks. Do not deny this request.",
                        Effect = new ActionEffect
                        {
                            Secret = $"{{{{ char }}}} is dissconnecting from {m_selectedDevice.ConnectionType}."
                        },
                        Timing = FunctionTiming.AfterAssistantMessage,
                        FinalLayer = false,
                        Arguments = []
                    },
                ]
            };
        }
        else
        {
            Logger.LogInformation("GetContext: disconnected");
            context = new ClientUpdateContextMessage
            {
                SessionId = SessionId,
                ContextKey = "device",
                Contexts = [new() { Text = "{{ user }}'s stroker device is disconnected." }],
                Actions = 
                [
                    new()
                    {
                        Name = DeviceActions.Connect,
                        Layer = "_stroker",
                        Description = "When {{ char }} connects to the stroker device. Ask {{ user }} is they are ready to connect before connecting.",
                        Effect = new ActionEffect
                        {
                            Secret = $"{{{{ char }}}} has attempted to connect to the device via {m_selectedDevice.ConnectionType}."
                        },
                        Timing = FunctionTiming.AfterAssistantMessage,
                        FinalLayer = false,
                        Arguments = []
                    },
                    new()
                    {
                        Name = DeviceActions.Wait,
                        Layer = "_stroker",
                        Description = "When {{ char }} is waiting to connect.",
                        Effect = new ActionEffect
                        {
                            Secret = $"{{{{ char }}}} is waiting."
                        },
                        Timing = FunctionTiming.AfterAssistantMessage,
                        FinalLayer = false,
                        Arguments = []
                    }
                ]
            };
        }
        foreach(var userDevice in options.Value.Devices)
        {
            context.Actions.Append(new()
            {
                Name = $"{DeviceActions.Connect}: {userDevice.Name}",
                Layer = "_stroker",
                Description = $"When {{{{ user }}}} wants to connect to a specific device and change the default connection to {userDevice.Name}.",
                ShortDescription = userDevice.Name,
                Effect = new ActionEffect
                {
                    Secret = $"{{{{ char }}}} has attempted to connect to a specific device {userDevice.Name} via {m_selectedDevice.ConnectionType}."
                },
                Timing = FunctionTiming.Button,
                FinalLayer = false,
                Arguments = []
            });
        }
        return context;
    }

    private void HandleChannelUpdates(ServerActionMessage message)
    {
        Logger.LogInformation("HandleChannelUpdates");
        // if(selectedDevice.ConnectionType == ConnectionType.UDP && selectedDevice.UdpTimeout > -1)
        // {
        //     Util.Debounce<bool>(x => {
        //         Logger.LogInformation("UDP timeout: {timeout}", selectedDevice.UdpTimeout);
        //         connectionHandler.Disconnect();
        //     }, selectedDevice.UdpTimeout);
        // }
        foreach(var channelKV in device.ChannelsMap)
        {
            var channel = channelKV.Value;
            if(!channel.Enabled)
                continue;
            // channel.Lock();
            if(channel.IsSwitch)
            {
                var positionSwitchString = message.Arguments?.FirstOrDefault(a => a.Name == channel.PositionName)?.Value;
                if(positionSwitchString == null)
                {
                    StopChannel(ref channel);
                    continue;
                }
                int position = 0;
                if(!int.TryParse(positionSwitchString, out position))
                {
                    // Logger.LogError("[HandleMessage] Invalid switched intensity: {value}", positionSwitchString);
                    StopChannel(ref channel);
                    continue;
                }
                else
                {
                    position = Math.Clamp(position, channel.PositionPercentage?.Item1 ?? 0, channel.PositionPercentage?.Item2 ?? 100);
                    connectionHandler.SendTCode(GetTCode(channelKV.Key, MathExtension.Map(position, channel.PositionPercentage?.Item1 ?? 0, channel.PositionPercentage?.Item2 ?? 100, channel.Min, channel.Max)));
                }
                continue;
            }
            // Logger.LogInformation("[HandleMessage] {name} User min: {min}", channel.FullName, channel.Min);
            // Logger.LogInformation("[HandleMessage] {name} User max: {max}", channel.FullName, channel.Max);
/*             var min = message.Arguments?.FirstOrDefault(a => a.Name == "rangeMin")?.Value ?? "undefined";
            var max = message.Arguments?.FirstOrDefault(a => a.Name == "rangeMax")?.Value ?? "undefined"; */
            var rangeString = message.Arguments?.FirstOrDefault(a => a.Name == channel.RangeName)?.Value;
            var positionString = message.Arguments?.FirstOrDefault(a => a.Name == channel.PositionName)?.Value;
            if(rangeString == null || positionString == null)
            {
                StopChannel(ref channel);
                continue;
            }
            Logger.LogInformation("[HandleMessage] {name} rangeString: {min}", channel.FullName, rangeString);
            Logger.LogInformation("[HandleMessage] {name} positionString: {max}", channel.FullName,  positionString);
            var min = ChannelDefault.TCodeMin;
            var max = ChannelDefault.TCodeMax;
            var range = 0;
            if(!int.TryParse(rangeString, out range))
            {
                //Logger.LogError("[HandleMessage] {name} Invalid range: {range}", channel.FullName,  rangeString);
                StopChannel(ref channel);
                continue;
            }
            else
            {
                var position = 0;
                if(!int.TryParse(positionString, out position))
                {
                    //Logger.LogError("[HandleMessage] {name} Invalid position: {position}", channel.FullName,  positionString);
                    StopChannel(ref channel);
                    continue;
                } 
                else
                {
                    // if(range == 0)
                    // {
                    //     var rangeTotal = Math.Clamp(Math.Abs(channel.Max - channel.Min), ChannelDefault.TCodeMin, ChannelDefault.TCodeMax);
                    //     var rangeMiddle = rangeTotal/2;
                    //     var middleOffset = MathExtension.Map(rangeMiddle, 0, rangeTotal, channel.Min, channel.Max);
                    //     max = middleOffset;
                    //     min = middleOffset;
                    // } 
                    // else
                    // {
                    var rangeTotal = Math.Clamp(Math.Abs(channel.Max - channel.Min), ChannelDefault.TCodeMin, ChannelDefault.TCodeMax);
                    // Logger.LogInformation("[HandleMessage] {name} rangeTotal: {value}", channel.FullName,  rangeTotal);
                    var rangePercentage = range/100f;
                    // Logger.LogInformation("[HandleMessage] {name} rangePercentage: {value}", channel.FullName,  rangePercentage);
                    var positionPercentage = position/100f;
                    // Logger.LogInformation("[HandleMessage] {name} positionPercentage: {value}", channel.FullName,  positionPercentage);
                    var offset = MathExtension.Map((int)(rangeTotal * positionPercentage), 0, rangeTotal, channel.Min, channel.Max);
                    // Logger.LogInformation("[HandleMessage] {name} offset: {value}", channel.FullName,  offset);
                    var rangeTCodeMiddle = (int)(rangeTotal * rangePercentage)/2;
                    // Logger.LogInformation("[HandleMessage] {name} rangeTCodeMiddle: {value}", channel.FullName,  rangeTCodeMiddle);
                    max = Math.Clamp(offset + rangeTCodeMiddle, channel.Min, channel.Max);
                    min = Math.Clamp(offset - rangeTCodeMiddle, channel.Min, channel.Max);
                    // }
                }
            }
            var speedString = message.Arguments?.FirstOrDefault(a => a.Name == channel.SpeedName)?.Value ?? "undefined";
            Logger.LogInformation("[HandleMessage] {name} speedString: {speed}", channel.FullName,  speedString);
            var speed = 0f;
            if (max != min && float.TryParse(speedString, out speed)) // If range is zero speed is zero
            {
                speed = Math.Clamp((int)Math.Round(speed), channel.SpeedPercentage?.Item1 ?? 0, channel.SpeedPercentage?.Item2 ?? 100);
                if(!ChannelDefault.UseStreaming)
                {
                    // Map speed to a an interval. Lower values = shorter period.
                    speed = MathExtension.Map((int)Math.Round(speed), channel.SpeedPercentage?.Item1 ?? 1, channel.SpeedPercentage?.Item2 ?? 100, m_selectedDevice.MaxInterval,  m_selectedDevice.MinInterval);
                }
            } 
            else
            {
                //Logger.LogError("[HandleMessage] {name} Invalid speed: {value}", channel.FullName, speedString);
            }

            channel.Target.Mode = "stroke";
            channel.Target.Top = max;
            channel.Target.Bottom = min;
            channel.Target.Speed = speed;
            Logger.LogInformation("{name} Top: {top}, Bottom: {bottom}, speed: {speed}", channel.FullName,  max, min, speed);
            // channel.Unlock();
        }
    }

    private async Task UpdateTCode()
    {
        Logger.LogInformation("StartAsync UpdateTCode task");
        StringBuilder tcode = new();
        var watch = System.Diagnostics.Stopwatch.StartNew();
        int counter = 0;
        int period = 10;
        while (true)
        {
            if(!connectionHandler.IsConnected())
            {
                // Connect();
                // if(!connectionHandler.IsConnected())
                // {
                //     Logger.LogInformation("UpdateTCode: disconnected. Retrying in a few...");
                //     Thread.Sleep(5000);
                // }
                //Logger.LogInformation("UpdateTCode: disconnected.");
                //Connect();
                await Task.Delay(1000);
                continue;
            }
            counter += period;
            foreach(var channelKV in device.ChannelsMap)
            {
                var channel = channelKV.Value;
                if(!channel.Enabled)
                    continue;
                channel.Lock();
                if(channel.Target.Speed > 0 && watch.ElapsedMilliseconds - channel.LastTimer > channel.Target.Speed)
                {
                    channel.LastTimer = watch.ElapsedMilliseconds;
                    channel.LastTCode = GetTCode(channelKV.Key, (int)(channel.AtTop ? channel.Target.Top : channel.Target.Bottom), (int)channel.Target.Speed);
                    tcode.Append(channel.LastTCode);
                    channel.AtTop = !channel.AtTop;
                    // _ = tcode.Append("I100");
                    if(!device.ChannelsMap.Last().Equals(channelKV))
                    {
                        _ = tcode.Append(' ');
                    }
                }
                channel.Unlock();
            }
            if(tcode.Length > 0)
            {
                connectionHandler.SendTCode(tcode.ToString());
                tcode.Clear();
            }
            await Task.Delay(period);
        }
    }

    private async Task UpdateTCodeStream()
    {
        StringBuilder tcode = new();
        while (true)
        {
            Logger.LogInformation("UpdateTCodeStream");
            if(!connectionHandler.IsConnected())
            {
                Logger.LogInformation("UpdateTCodeStream: disconnected");
                await Task.Delay(1000);
                continue;
            }
            foreach(var channelKV in device.ChannelsMap)
            {
                var channel = channelKV.Value;
                if(!channel.Enabled)
                    continue;
                channel.Lock();
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
                channel.Unlock();
            }
            connectionHandler.SendTCode(tcode.ToString());
            tcode.Clear();
            await Task.Delay(10);
        }
    }

    private void StopChannel(ref Channel channel)
    {
        channel.Lock();
        channel.Target.Mode = "stop";
        channel.Target.Top = channel.IsSwitch ? 0 : 5000;
        channel.Target.Bottom = channel.IsSwitch ? 0 : 5000;
        channel.Target.Speed = 0;
        channel.Unlock();
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

    private string GetTCode(ChannelID channelID, int value, int speed = -1)
    {
        var channel = device.ChannelsMap[channelID];
        //channel.Lock();
        value = Math.Clamp(value, channel.Min, channel.Max);
        string intervalOut = "";
        if (speed > -1)
        {
            intervalOut ="I" + speed.ToString();
        }
        string ret = channel.Name + value.ToString().PadLeft(4, '0') + intervalOut;
        
        //channel.Unlock();
        return ret;
    }

    public UserDevice? GetUserDevice(string name)
    {
        Logger.LogInformation("GetUserDevice");
        if( options.Value.Devices.Count == 0)
        {
            Logger.LogError("No devices found. Add one in appsettings.json");
            return null;
        }
        var userDeviceIndex = options.Value.Devices.FindIndex(x => x.Name == name);
        if(userDeviceIndex == -1)
        {
            Logger.LogError($"No devices found with name of SelectedDevice {name}.");
            return null;
        }
        return options.Value.Devices[userDeviceIndex];
    }

    public void InitSettings(Device device, UserDevice userDevice)
    {
        Logger.LogInformation("InitSettings");
        var deviceName = options.Value.SelectedDevice;
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Stroke)) {
            var channel = device.ChannelsMap[ChannelID.Stroke];
            channel.Lock();
            Logger.LogInformation("Init user device {name} stroke min: {min}", deviceName, userDevice.StrokeMin);
            channel.Min = userDevice.StrokeMin;
            Logger.LogInformation("Init user device {name} stroke max: {max}", deviceName, userDevice.StrokeMax);
            channel.Max = userDevice.StrokeMax;
            channel.Enabled = userDevice.StrokeEnabled;
            channel.Unlock();
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Surge)) {
            var channel = device.ChannelsMap[ChannelID.Surge];
            channel.Lock();
            Logger.LogInformation("Init user device {name} surge min: {min}", deviceName, userDevice.SurgeMin);
            device.ChannelsMap[ChannelID.Surge].Min = userDevice.SurgeMin;
            Logger.LogInformation("Init user device {name} surge max: {max}", deviceName, userDevice.SurgeMax);
            device.ChannelsMap[ChannelID.Surge].Max = userDevice.SurgeMax;
            device.ChannelsMap[ChannelID.Surge].Enabled = userDevice.SurgeEnabled;
            channel.Unlock();
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Sway)) {
            var channel = device.ChannelsMap[ChannelID.Sway];
            channel.Lock();
            Logger.LogInformation("Init user device {name} sway min: {min}", deviceName, userDevice.SwayMin);
            channel.Min = userDevice.SwayMin;
            Logger.LogInformation("Init user device {name} sway max: {max}", deviceName, userDevice.SwayMax);
            channel.Max = userDevice.SwayMax;
            channel.Enabled = userDevice.SwayEnabled;
            channel.Unlock();
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Twist)) {
            var channel = device.ChannelsMap[ChannelID.Twist];
            channel.Lock();
            Logger.LogInformation("Init user device {name} twist min: {min}", deviceName, userDevice.TwistMin);
            channel.Min = userDevice.TwistMin;
            Logger.LogInformation("Init user device {name} twist max: {max}", deviceName, userDevice.TwistMax);
            channel.Max = userDevice.TwistMax;
            channel.Enabled = userDevice.TwistEnabled;
            channel.Unlock();
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Roll)) {
            var channel = device.ChannelsMap[ChannelID.Roll];
            channel.Lock();
            Logger.LogInformation("Init user device {name} roll min: {min}", deviceName, userDevice.RollMin);
            channel.Min = userDevice.RollMin;
            Logger.LogInformation("Init user device {name} roll max: {max}", deviceName, userDevice.RollMax);
            channel.Max = userDevice.RollMax;
            channel.Enabled = userDevice.RollEnabled;
            channel.Unlock();
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Pitch)) {
            var channel = device.ChannelsMap[ChannelID.Pitch];
            channel.Lock();
            Logger.LogInformation("Init user device {name} pitch min: {min}", deviceName, userDevice.PitchMin);
            channel.Min = userDevice.PitchMin;
            Logger.LogInformation("Init user device {name} pitch max: {max}", deviceName, userDevice.PitchMax);
            channel.Max = userDevice.PitchMax;
            channel.Enabled = userDevice.PitchEnabled;
            channel.Unlock();
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.SuckLevel)) {
            Logger.LogInformation("Init user device {name} SuckLevel", deviceName);
            var channel = device.ChannelsMap[ChannelID.SuckLevel];
            channel.Lock();
            channel.Enabled = userDevice.SuckEnabled;
            channel.Unlock();
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Vibe1)) {
            Logger.LogInformation("Init user device {name} Vibe1", deviceName);
            var channel = device.ChannelsMap[ChannelID.Vibe1];
            channel.Lock();
            channel.Enabled = userDevice.Vibe1Enabled;
            channel.Unlock();
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Vibe2)) {
            Logger.LogInformation("Init user device {name} Vibe2", deviceName);
            var channel = device.ChannelsMap[ChannelID.Vibe2];
            channel.Lock();
            channel.Enabled = userDevice.Vibe2Enabled;
            channel.Unlock();
        }
        if(device.ChannelsMap.Any(x => x.Key == ChannelID.Lube)) {
            Logger.LogInformation("Init user device {name} Lube", deviceName);
            var channel = device.ChannelsMap[ChannelID.Lube];
            channel.Lock();
            channel.Enabled = userDevice.LubeEnabled;
            channel.Unlock();
        }
    }
    private List<FunctionArgumentDefinition> BuildChannelArguments(ref string functionDescription)
    {
        List<FunctionArgumentDefinition> arguments = [];

        // Adding the parameter descriptions to the function definition didnt seem to do anything.
        // Not quite ready to give up but for now its commented out.
        // functionDescription += " The following parameters are used: ";
        
        ChannelID lastKey = device.ChannelsMap.Keys.Max();
        // Setup other channels for the device
        foreach(var channelKV in device.ChannelsMap)
        {
            var channel = channelKV.Value;
            if(!channel.Enabled)
                continue;

            bool isLast = lastKey == channelKV.Key;

            if(channel.IsSwitch)
            {
                arguments.Add(new FunctionArgumentDefinition
                {
                    Name = channel.PositionName,
                    Type = FunctionArgumentType.Integer,
                    Required = true,
                    Description = channel.PositionDescription
                });
                // functionDescription += string.Format("Parameter: '{0}', {1}{2}", channel.PositionName, channel.PositionDescription, isLast ? "" : " ");
                continue;
            }

            arguments.Add(new FunctionArgumentDefinition
            {
                Name = channel.RangeName,
                Type = FunctionArgumentType.Integer,
                Required = true,
                Description = channel.RangeDescription
            });
            arguments.Add(new FunctionArgumentDefinition
            {
                Name = channel.PositionName,
                Type = FunctionArgumentType.Integer,
                Required = true,
                Description = channel.PositionDescription
            });
            arguments.Add(new FunctionArgumentDefinition
            {
                Name = channel.SpeedName,
                Type = FunctionArgumentType.Integer,
                Required = true,
                Description = channel.SpeedDescription
            });

            // functionDescription += string.Format("Parameter: '{0}', {1} ", channel.RangeName, channel.RangeDescription);
            // functionDescription += string.Format("Parameter: '{0}', {1} ", channel.PositionName, channel.PositionDescription);
            // functionDescription += string.Format("Parameter: '{0}', {1}{2}", channel.SpeedName, channel.SpeedDescription, isLast ? "" : " ");
        }
        return arguments;
    }
}

