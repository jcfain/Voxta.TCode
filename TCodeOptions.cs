using System.ComponentModel.DataAnnotations;

namespace Voxta.TCode;

[Serializable]
public class TCodeOptions
{
    [Required]
    public required bool UseUDP { get; init; }
    [Required]
    public required string SerialPort { get; init; }
    [Required]
    public required string UDPAddress { get; init; }
    [Required]
    public required int UDPPort { get; init; }
    [Required]
    public required int AutoReplyDelay { get; init; }
    [Required]
    public required int StrokeMin { get; init; }
    [Required]
    public required int StrokeMax { get; init; }
}
