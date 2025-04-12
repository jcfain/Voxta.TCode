namespace Voxta.TCode.Model
{
    public enum ChannelID
    {
        Stroke,
        Surge,
        Sway,
        Twist,
        Roll,
        Pitch,
        SuckManual,
        SuckLevel,
        Lube,
        Vibe1,
        Vibe2
    }

    // const Channel Stroke = {"L0", "Stroke", false, false, TCODE_MIN, TCODE_MID, TCODE_MAX};
    // const Channel Surge = {"L1", "Surge", false, true, TCODE_MIN, TCODE_MID, TCODE_MAX};
    // const Channel Sway = {"L2", "Sway", false, true, TCODE_MIN, TCODE_MID, TCODE_MAX};
    // const Channel Twist = {"R0", "Twist", false, false, TCODE_MIN, TCODE_MID, TCODE_MAX};
    // const Channel Roll = {"R1", "Roll", false, false, TCODE_MIN, TCODE_MID, TCODE_MAX};
    // const Channel Pitch = {"R2", "Pitch", false, false, TCODE_MIN, TCODE_MID, TCODE_MAX};
    // const Channel Vibe1 = {"V0", "Vibe 1", true, false, TCODE_MIN, TCODE_MID, TCODE_MAX};
    // const Channel Vibe2 = {"V1", "Vibe 2", true, false, TCODE_MIN, TCODE_MID, TCODE_MAX};
    // const Channel Vibe3 = {"V2", "Vibe 3", true, false, TCODE_MIN, TCODE_MID, TCODE_MAX};
    // const Channel Vibe4 = {"V3", "Vibe 4", true, false, TCODE_MIN, TCODE_MID, TCODE_MAX};
    // const Channel SuckManual = {"A0", "Suck manual", false, false, TCODE_MIN, TCODE_MID, TCODE_MAX};
    // const Channel SuckLevel = {"A1", "Suck level", false, false, TCODE_MIN, TCODE_MID, TCODE_MAX};
    // const Channel Lube = {"A2", "Lube", true, false, TCODE_MIN, TCODE_MIN, TCODE_MAX};
    // const Channel Squeeze = {"A3", "Squeeze", false, false, TCODE_MIN, TCODE_MID, TCODE_MAX};
    public class ChannelName
    {
        public static readonly string Stroke = "L0";
        public static readonly string Surge = "L1";
        public static readonly string Sway = "L2";
        public static readonly string Twist = "R0";
        public static readonly string Roll = "R1";
        public static readonly string Pitch = "R2";
        public static readonly string SuckManual = "A0";
        public static readonly string SuckLevel = "A1";
        public static readonly string Lube = "A2";
        public static readonly string Vibe1 = "V1";
        public static readonly string Vibe2 = "V2";
        // public static readonly string Vibe3 = "V3";
        // public static readonly string Vibe4 = "V4";
        public static readonly Dictionary<ChannelID, string> Map = new(){
            {ChannelID.Stroke, ChannelName.Stroke},
            {ChannelID.Surge, ChannelName.Surge},
            {ChannelID.Sway, ChannelName.Sway},
            {ChannelID.Twist, ChannelName.Twist},
            {ChannelID.Roll, ChannelName.Roll},
            {ChannelID.Pitch, ChannelName.Pitch},
            {ChannelID.SuckManual, ChannelName.SuckManual},
            {ChannelID.SuckLevel, ChannelName.SuckLevel},
            {ChannelID.Lube, ChannelName.Lube},
            {ChannelID.Vibe1, ChannelName.Vibe1},
            {ChannelID.Vibe2, ChannelName.Vibe2}
            // {ChannelID.Pitch, ChannelName.Vibe3},
            // {ChannelID.Pitch, ChannelName.Vibe4},
        };

    }
    public class Channel(string name, string fullName, string rangeDescription = "", string positionDescription = "", string speedDescription = "", bool isSwitch = false)
    {
        public string Name = name;
        public string FullName = fullName;
        public string RangeDescription = rangeDescription;
        public string PositionDescription = positionDescription;
        public string SpeedDescription = speedDescription;
        public bool IsSwitch = isSwitch;
        public bool Enabled = true;
        public ChannelTarget Target = new();
        public float Speed = 0;
        public float Top = 5000;
        public float Bottom = 5000;
        public float Timer = 0;
        public int Min = 0;
        public int Max = 0;

    }
}