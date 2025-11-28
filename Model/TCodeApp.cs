
using System;
using Voxta.Model.Shared;
using Voxta.Model.Shared.Forms;

namespace Voxta.Model.WebsocketMessages.ClientMessages;

[Serializable]
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