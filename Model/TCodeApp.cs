
using Voxta.Model.Shared;
using Voxta.Model.WebsocketMessages.ClientMessages;

public class TCodeApp : ClientMessage
{
    public string? ClientVersion { get; init; }
    public string? IconBase64Url { get; init;  }
    public string? Label { get; init; }
    public Form? ConfigurationForm { get; init; }
    public Form? CharacterForm { get; init; }
    public Form? ScenarioForm { get; init; }
    public ScriptSnippet[]? ScriptSnippets { get; init; }
}