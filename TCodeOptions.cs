using System.ComponentModel.DataAnnotations;
using Voxta.TCode.Model;

namespace Voxta.TCode;

[Serializable]
public class TCodeOptions
{
    [Required]
    public required ConnectionType ConnectionType { get; init; }
    [Required]
    public required string SerialPort { get; init; }
    [Required]
    public required string NetworkAddress { get; init; }
    [Required]
    public required int NetworkPort { get; init; }
    [Required]
    public required int AutoReplyDelay { get; init; }
    [Required]
    public required DeviceType DeviceType { get; init; }
    [Required]
    public required bool StrokeEnabled { get; init; }
    [Required]
    public required int StrokeMin { get; init; }
    [Required]
    public required int StrokeMax { get; init; }
    [Required]
    public required bool SurgeEnabled { get; init; }
    [Required]
    public required int SurgeMin { get; init; }
    [Required]
    public required int SurgeMax { get; init; }
    [Required]
    public required bool SwayEnabled { get; init; }
    [Required]
    public required int SwayMin { get; init; }
    [Required]
    public required int SwayMax { get; init; }
    [Required]
    public required bool TwistEnabled { get; init; }
    [Required]
    public required int TwistMin { get; init; }
    [Required]
    public required int TwistMax { get; init; }
    [Required]
    public required bool RollEnabled { get; init; }
    [Required]
    public required int RollMin { get; init; }
    [Required]
    public required int RollMax { get; init; }
    [Required]
    public required bool PitchEnabled { get; init; }
    [Required]
    public required int PitchMin { get; init; }
    [Required]
    public required int PitchMax { get; init; }
    [Required]
    public required bool SuckEnabled { get; init; }
    [Required]
    public required bool LubeEnabled { get; init; }
    [Required]
    public required bool Vibe1Enabled { get; init; }
    [Required]
    public required bool Vibe2Enabled { get; init; }
}
