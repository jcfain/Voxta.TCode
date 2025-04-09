using Microsoft.Extensions.Logging;
using Voxta.Model.Shared;
using Voxta.Model.WebsocketMessages.ClientMessages;
using Voxta.Model.WebsocketMessages.ServerMessages;
using Voxta.Providers.Host;

namespace Voxta.SampleProviderApp.Providers;

// This example shows how to create and act on character action inference.
// Note that this is typically not for user commands, another system will be released later
public class ActionProvider(
    IRemoteChatSession session,
    ILogger<ActionProvider> logger
) : ProviderBase(session, logger)
{
    SerialPort serial;

    private float targetStrokeSpeed = 0;
    private int targetStrokeTop = 9999;
    private int targetStrokeBottom = 9999;
    private string strokeMode = "Off";
    private float strokeSpeed = 0;
    private float strokeTop = 9999;
    private float strokeBottom = 9999;
    private float strokeTimer = 0;


    public ActionProvider(
        IRemoteChatSession session,
        ILogger<ActionProvider> logger)
        : base(session, logger)
    {
        serial = new SerialPort("COM3", 115200);
        serial.Open();
        serial.DtrEnable = true;
        serial.ReadTimeout = 10;
        Console.WriteLine("Serial connection started");
    }

    protected override async Task OnStartAsync()
    {
        await base.OnStartAsync();

        // Register our action
        Send(new ClientUpdateContextMessage
        {
            SessionId = SessionId,
            ContextKey = "SampleActions",
            Actions = 
            [
                new()
                {
                    // The LLM will use this name to call the action, use a good action name
                    Name = "long_strokes",
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
                    // The LLM will use this name to call the action, use a good action name
                    Name = "short strokes",
                    // Layers allow you to run your actions separately from the scene
                    Layer = "_stroker",
                    // Helps the AI understand when and how to use the function
                    Description = "When {{ char }} wants to give intense pleasure to {{ user }} sexually or go really fast.",
                    // This text will be prepended to the AI's response
                    Effect = "{{ char }} is giving {{ user }} intense sexual stimulation.",
                    // Optional arguments for your action
                    Arguments = new[]
                    {
                        new FunctionArgumentDefinition
                        {
                            // The name sent to the LLM
                            Name = "speed",
                            // The type of argument
                            Type = FunctionArgumentType.Integer,
                            // Whether to access failure to generate this argument
                            Required = true,
                            // Explanation for the LLM
                            Description = "The intensity of the short strokes. It can be a number 1-10."
                        }
                    }
                },
                new()
                {
                    // The LLM will use this name to call the action, use a good action name
                    Name = "teasing_strokes",
                    // Layers allow you to run your actions separately from the scene
                    Layer = "_stroker",
                    // Helps the AI understand when and how to use the function
                    Description = "When {{ char }} wants to give gentle pleasure to {{ user }} sexually or go slow.",
                    // This text will be prepended to the AI's response
                    Effect = "{{ char }} is giving {{ user }} gentle sexual stimulation.",
                    // Optional arguments for your action
                    Arguments = new[]
                    {
                        new FunctionArgumentDefinition
                        {
                            // The name sent to the LLM
                            Name = "speed",
                            // The type of argument
                            Type = FunctionArgumentType.Integer,
                            // Whether to access failure to generate this argument
                            Required = true,
                            // Explanation for the LLM
                            Description = "The intensity of the teasing strokes. It can be a number 1-10."
                        }
                    }
                },
                new()
                {
                    // The LLM will use this name to call the action, use a good action name
                    Name = "stop",
                    // Layers allow you to run your actions separately from the scene
                    Layer = "_stroker",
                    // Helps the AI understand when and how to use the function
                    Description = "When {{ char }} wants to tease {{ user }} by denying him stimulation. Or if {{ user }} wants {{ char}} to stop.",
                    // This text will be prepended to the AI's response
                    Effect = "{{ char }} has stopped stimulating {{ user }}.",
                    // Optional arguments for your action
                    Arguments = new[]
                    {
                        new FunctionArgumentDefinition
                        {
                            // The name sent to the LLM
                            Name = "speed",
                            // The type of argument
                            Type = FunctionArgumentType.Integer,
                            // Whether to access failure to generate this argument
                            Required = true,
                            // Explanation for the LLM
                            Description = "The strength of the denial. It can be a number 1-10."
                        }
                    }
                },
            }
        });

        // Act when an action is called
        HandleMessage<ServerActionMessage>(message =>
        {
            // We only care about our layer
            if (message.Layer != "_stroker") return;
            var speed = message.Arguments?.FirstOrDefault(a => a.Name == "speed")?.Value ?? "undefined";

            switch (message.Value)
            {
                case "long_strokes":
                    strokeMode = "long_strokes";
                    targetStrokeTop = 9999;
                    targetStrokeBottom = 1000;
                    Logger.LogInformation("Long strokes, speed: {speed}", speed);
                    targetStrokeSpeed = Convert.ToSingle(Math.Clamp(Int32.Parse(speed), 1, 10));
                    break;

                case "short strokes":
                    strokeMode = "short_strokes";
                    targetStrokeTop = 5000;
                    targetStrokeBottom = 0;
                    Logger.LogInformation("Short strokes, speed {speed}", speed);
                    targetStrokeSpeed = 5f + 1.5f * Convert.ToSingle(Math.Clamp(Int32.Parse(speed), 1, 10));
                    break;

                case "teasing_strokes":
                    strokeMode = "teasing_strokes";
                    targetStrokeTop = 9999;
                    targetStrokeBottom = 6000;
                    Logger.LogInformation("Teasing strokes, speed: {speed}", speed);
                    targetStrokeSpeed = 0.6f * Convert.ToSingle(Math.Clamp(Int32.Parse(speed), 1, 10));
                    break;

                case "stop":
                default:
                    strokeMode = "stop";
                    targetStrokeTop = 9999;
                    targetStrokeBottom = 0;
                    Logger.LogInformation("Stop, speed: {speed}", speed);
                    targetStrokeSpeed = Convert.ToSingle(Math.Clamp(Int32.Parse(speed), 1, 10));
                    break;

            }

        });

        Task backgroundTask = UpdateTCode();
    }

    private async Task UpdateTCode()
    {
        while (true)
        {

            strokeSpeed = converge(strokeSpeed, targetStrokeSpeed, 0.05f);
            strokeTop = converge(strokeTop, targetStrokeTop, 20f);
            strokeBottom = converge(strokeBottom, targetStrokeBottom, 20f);

            strokeTimer += strokeSpeed * 1.2f * 0.0094248f; // ie speed of 20 = 3 strokes per second
            if (strokeTimer > 6.2831853f)
            {
                if (strokeMode == "stop")
                {
                    strokeTimer = 6.2831853f;
                    strokeSpeed = 0;
                }
                else
                {
                    strokeTimer -= 6.2831853f;
                }
            }

            float floatPos = (strokeTop + strokeBottom + (strokeTop - strokeBottom) * Convert.ToSingle(Math.Cos(strokeTimer))) / 2;
            int pos = Convert.ToInt32(floatPos + 0.5f);

            serial.WriteLine(GetTCode("L0", pos) + "\n");
            await Task.Delay(10);
        }
    }

    float converge(float input, float target, float rate)
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

    string GetTCode(string axis, int value)
    {
        value = Math.Clamp(value, 0, 9999);
        string output;
        if (value > 999)
        {
            output = axis + value.ToString();
        }
        else if (value > 99)
        {
            output = axis + "0" + value.ToString();
        }
        else if (value > 9)
        {
            output = axis + "00" + value.ToString();
        }
        else
        {
            output = axis + "000" + value.ToString();
        }
        return output;
    }
}

