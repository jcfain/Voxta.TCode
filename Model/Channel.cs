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
        public static readonly string IntensityDescription = "The speed of the {0}. When {{char}} wants to change the rate of the {0}. Higher numbers is a faster {0}. It can be a number {1}-{2}.";
        public static readonly string RangeDescription = "The range of the {0}. It can be a number {1}-{2}.";
        public static readonly string PositionDescription = "The offset position of the {0} range. This value will oscillate in the specified range if the intensity is greater than 0. It can be a number {1}-{2}.";
        public static readonly string IntensityName = "{0}Intensity";
        public static readonly string RangeName = "{0}Range";
        public static readonly string PositionName= "{0}Position";
        public static readonly int TCodeMin = 0;
        public static readonly int TCodeMax = 9999;
        public static readonly Channel Stroke = new()
        {
            Name = ChannelName.Stroke, 
            FullName = "stroke", 
            IntensityPercentage = Tuple.Create(0, 10),
            RangePercentage = Tuple.Create(30, 100),
            PositionPercentage = Tuple.Create(10, 90), 
            IntensityDescription = "The speed of the stroke. When {{char}} wants to change the rate of the stroke. Higher numbers is faster strokes. It can be a number {1}-{2}.",
            RangeDescription = "The range of the {0}. When {{char}} wants to stroke the cock of {{user}}. This value is how long the stroke is. Larger numbers, the longer the stroke. This value is relative to the position value. It can be a number {1}-{2}.", 
            PositionDescription = "The offset of the {0} range. Where higher numbers is the head or tip of the cock and lower numbers is the base of the cock. It can be a number {1}-{2}."
        };
        public static readonly Channel Surge = new()
        {
            Name = ChannelName.Surge, 
            FullName = "surge",
            IntensityPercentage = Tuple.Create(0, 10),
            PositionPercentage = Tuple.Create(10, 90),
            RangePercentage = Tuple.Create(10, 100),
            RangeDescription = "The range of the {0}. This value moves the cock back and forth. Use this when {{char}} wants to move the cock to the back and forth. For example, when {{char}} hips are thrusting on the cock. It can be a number {1}-{2}."
        };
        public static readonly Channel Sway = new()
        {
            Name = ChannelName.Sway, 
            FullName = "sway",
            IntensityPercentage = Tuple.Create(0, 10),
            PositionPercentage = Tuple.Create(10, 90),
            RangePercentage = Tuple.Create(10, 100),
            RangeDescription = "The range of the {0}. This value moves the cock side to side. Use this on {{user}} when {{char}} wants to move the cock to the left or right. For example when the hips sway side to side. It can be a number {1}-{2}."
        };
        public static readonly Channel Twist = new()
        {
            Name = ChannelName.Twist, 
            FullName = "twisting",
            IntensityPercentage = Tuple.Create(0, 10),
            PositionPercentage = Tuple.Create(10, 90),
            RangePercentage = Tuple.Create(10, 100),
            RangeDescription = "The range of the {0}. This value moves is yaw that twists around the cock. Use this on {{user}} when the {{char}} wants to use their tongue or twist their hand around the cock. It can be a number {1}-{2}."
        };
        public static readonly Channel Roll = new()
        {
            Name = ChannelName.Roll, 
            FullName = "roll",
            IntensityPercentage = Tuple.Create(0, 10),
            PositionPercentage = Tuple.Create(10, 90),
            RangePercentage = Tuple.Create(10, 100),
            RangeDescription = "The range of the {0}. This value rolls side to side over the cock. Uses this on {{user}} when {{char}} is roling hips or head while giving head. It can also be used when giving a handjob. It can be a number {1}-{2}."
        };
        public static readonly Channel Pitch = new()
        {
            Name = ChannelName.Pitch, 
            FullName = "pitch",
            IntensityPercentage = Tuple.Create(0, 10),
            PositionPercentage = Tuple.Create(10, 90),
            RangePercentage = Tuple.Create(10, 100),
            RangeDescription = "The range of the {0}. This value rolls side to side over the cock. This can be used when {{char}} is pitching their hips, hand or head whne pleasuring the {{user}}s cock. It can be a number {1}-{2}."
        };
        public static readonly Channel Suck = new()
        {
            Name = ChannelName.SuckLevel, 
            FullName = "suck",
            PositionPercentage =  Tuple.Create(0, 100),
            PositionDescription = "How much suction to generate stroking upwards. This can be used when {{char}}  is sucking the cock of {{user}}. Higher values is more suck. It can be a number {1}-{2}.", 
        };        
        public static readonly Channel Lube = new()
        {
            Name = ChannelName.Lube, 
            FullName = "lube",
            IntensityPercentage = Tuple.Create(0, 100),
            IntensityDescription = "How much lube to coat the cock with. This can be used when starting things like sucking the cock or vagina insertion. It can be a number {1}-{2}.", 
            IsSwitch = true
        };
        public static readonly Channel Vibe1 = new()
        {
            Name = ChannelName.Vibe1, 
            FullName = "vibrator number 1",
            IntensityPercentage = Tuple.Create(0, 100),
            IntensityDescription = "The intensity of the first vibrator. It can be a number {1}-{2}.", 
            IsSwitch = true
        };
        public static readonly Channel Vibe2 = new()
        {
            Name = ChannelName.Vibe2, 
            FullName = "vibrator number 2",
            IntensityPercentage = Tuple.Create(0, 100),
            IntensityDescription = "The intensity of the second vibrator. It can be a number {1}-{2}.", 
            IsSwitch = true
        };
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
    public class Channel
    {
        public required string Name {get;set;}
        public required string FullName {get;set;}
        public Tuple<int, int>? RangePercentage {get;set;}
        public Tuple<int, int>? PositionPercentage {get;set;}
        public Tuple<int, int>? IntensityPercentage {get;set;}
        public string RangeName {get { return string.Format(ChannelDefault.RangeName, FullName); }}
        public string  PositionName {get { return string.Format(ChannelDefault.PositionName, FullName); }}
        public string  IntensityName {get { return string.Format(ChannelDefault.IntensityName, FullName); }}
        public string RangeDescription 
        { 
            get
            {
                if(m_rangeDescription.Length == 0)
                    m_rangeDescription = string.Format(ChannelDefault.RangeDescription, FullName, RangePercentage?.Item1 ?? 0, RangePercentage?.Item2 ?? 100);
                return m_rangeDescription;
            }
            set 
            { 
                m_rangeDescription = string.Format(value, FullName, RangePercentage?.Item1 ?? 0, RangePercentage?.Item2 ?? 100);
            }
        }
        public string PositionDescription
        {
            get
            {
                if(m_positionDescription.Length == 0)
                    m_positionDescription = string.Format(ChannelDefault.PositionDescription, FullName, PositionPercentage?.Item1 ?? 0, PositionPercentage?.Item2 ?? 100);
                return m_positionDescription;
            }
            set 
            { 
                m_positionDescription = string.Format(value, FullName, PositionPercentage?.Item1 ?? 0, PositionPercentage?.Item2 ?? 100); 
            }
        }
        public string IntensityDescription
        {
            get
            {
                if(m_intensityDescription.Length == 0)
                    m_intensityDescription = string.Format(ChannelDefault.IntensityDescription, FullName, IntensityPercentage?.Item1 ?? 0, IntensityPercentage?.Item2 ?? 100);
                return m_intensityDescription;
            }
            set 
            { 
                m_intensityDescription = string.Format(value, FullName, IntensityPercentage?.Item1 ?? 0, IntensityPercentage?.Item2 ?? 100);}
            }
        public bool IsSwitch = false;
        public bool Enabled = true;
        public ChannelTarget Target = new();
        public float Speed = 0;
        public float Top = 5000;
        public float Bottom = 5000;
        public float Timer = 0;
        public int Min = 0;
        public int Max = 9999;
        private string m_rangeDescription = "";
        private string m_positionDescription = "";
        private string m_intensityDescription = "";
    }
}