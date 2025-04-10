namespace Voxta.TCode.Model
{
    public enum ChannelID
    {
        Stroke,
        Surge,
        Sway,
        Twist,
        Roll,
        Pitch
    }

    public class ChannelName
    {
        public static readonly string Stroke = "L0";
        public static readonly string Surge = "L1";
        public static readonly string Sway = "L2";
        public static readonly string Twist = "R0";
        public static readonly string Roll = "R1";
        public static readonly string Pitch = "R2";
        public static readonly Dictionary<ChannelID, string> Map = new(){
            {ChannelID.Stroke, ChannelName.Stroke},
            {ChannelID.Surge, ChannelName.Surge},
            {ChannelID.Sway, ChannelName.Sway},
            {ChannelID.Twist, ChannelName.Twist},
            {ChannelID.Roll, ChannelName.Roll},
            {ChannelID.Pitch, ChannelName.Pitch},
        };

    }
    public class Channel(string name, string fullName)
    {
        public string Name = name;
        public string FullName = fullName;
        public ChannelTarget Target = new();
        public float Speed = 0;
        public float Top = 9999;
        public float Bottom = 9999;
        public float Timer = 0;
        public int Min = 0;
        public int Max = 0;

    }
}