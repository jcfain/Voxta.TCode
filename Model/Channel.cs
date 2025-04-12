using System.Dynamic;
using System.Reflection;

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

    public class ChannelDefault
    {
        public static readonly string DefaultIntensityDescription = "The intensity of the {0}. It can be a number {1}-{2}.";
        public static readonly string DefaultRangeDescription = "The range of the {0}. It can be a number {0}-{1}.";
        public static readonly string DefaultPositionDescription = "The position of the {0} range. It can be a number {0}-{1}.";
        public static readonly string DefaultSpeedDescription = "The intensity of the {0}. It can be a number {0}-{1}.";
        public static readonly Channel Stroke = new{Name = ChannelName.Stroke, FullName = "stroke", 
                            IntensityPercentage = Tuple.Create(1, 10),
                            RangePercentage = Tuple.Create(30, 100),
                            Tuple.Create(10, 100), 
                            "The intensity of the {0}. It can be a number {1}-{2}.",
                            "The range of the {0}. It can be a number {1}-{2}.", 
                            "The position of the {0} range. Where 90 is the head or tip of the cock, 10 is the base of the cock and 50 is the middle of the cock. It can be a number {1}-{2}.");
        public static readonly Channel Surge = new(ChannelName.Surge, "surge",
                            Tuple.Create(1, 10),
                            Tuple.Create(1, 100),
                            Tuple.Create(10, 100));
        public static readonly Channel Sway = new(ChannelName.Sway, "sway",
                            Tuple.Create(1, 10),
                            Tuple.Create(1, 100),
                            Tuple.Create(10, 100));
        public static readonly Channel Twist = new(ChannelName.Twist, "twisting",
                            Tuple.Create(1, 10),
                            Tuple.Create(1, 100),
                            Tuple.Create(10, 100));
        public static readonly Channel Roll = new(ChannelName.Roll, "roll",
                            Tuple.Create(1, 10),
                            Tuple.Create(1, 100),
                            Tuple.Create(10, 100));
        public static readonly Channel Pitch = new(ChannelName.Roll, "pitch",
                            Tuple.Create(1, 10),
                            Tuple.Create(1, 100),
                            Tuple.Create(10, 100));
        public static readonly Channel Suck = new(ChannelName.SuckLevel, "suck",
                            Tuple.Create(0, 0),
                            Tuple.Create(0, 100),
                            Tuple.Create(0, 0), 
                            "", 
                            "How much suction to generate. It can be a number {1}-{2}.", 
                            "");
        public static readonly Channel Lube = new(ChannelName.Lube, "lube",
                            Tuple.Create(0, 0),
                            Tuple.Create(0, 100),
                            Tuple.Create(0, 0), 
                            "", 
                            "How much lube to coat the cock with. It can be a number {1}-{2}.", 
                            "",
                            true);
        public static readonly Channel Vibe1 = new(ChannelName.Vibe1, "vibrator number 1",
                            Tuple.Create(0, 0),
                            Tuple.Create(0, 100),
                            Tuple.Create(0, 0), 
                            "The intensity of the first vibrator. It can be a number {1}-{2}.", 
                            "", 
                            "",
                            true);
        public static readonly Channel Vibe2 = new(ChannelName.Vibe2, "vibrator number 2",
                            Tuple.Create(0, 0),
                            Tuple.Create(0, 100),
                            Tuple.Create(0, 0), 
                            "The intensity of the second vibrator. It can be a number {1}-{2}.", 
                            "", 
                            "",
                            true);
    }
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
    public interface IChannel {
        public string GetRangeDescription();
        public string GetPositionDescription();
        public string GetSpeedDescription();
        public string GetIntensityDescription();
    }
    public class Channel
    {
        public string Name {get;set;}
        public string FullName {get;set;}
        public Tuple<int, int> RangePercentage {get;set;}
        public Tuple<int, int> PositionPercentage {get;set;}
        public Tuple<int, int> IntensityPercentage {get;set;}
        public bool IsSwitch {get;set;}
        public string RangeDescription 
        { 
            get
            {
                return string.Format(m_rangeDescription.Length > 0 ? m_rangeDescription : ChannelDefault.DefaultRangeDescription, FullName, RangePercentage.Item1, RangePercentage.Item2);
            }
            set { m_rangeDescription = value; }
        }
        public string PositionDescription
        {
            get
            {
                return string.Format(m_positionDescription.Length > 0 ? m_positionDescription : ChannelDefault.DefaultPositionDescription, FullName, PositionPercentage.Item1, PositionPercentage.Item2);
            }
            set { m_positionDescription = value; }
        }
        public string IntensityDescription
        {
            get
            {
                return string.Format(m_intensityDescription.Length > 0 ? m_intensityDescription : ChannelDefault.DefaultSpeedDescription, FullName, IntensityPercentage.Item1, IntensityPercentage.Item2);
            }
            set { m_intensityDescription = value; }
        }
        public bool Enabled = true;
        public ChannelTarget Target = new();
        public float Speed = 0;
        public float Top = 5000;
        public float Bottom = 5000;
        public float Timer = 0;
        public int Min = 0;
        public int Max = 0;
        private string m_rangeDescription = "";
        private string m_positionDescription = "";
        private string m_intensityDescription = "";
    }
}