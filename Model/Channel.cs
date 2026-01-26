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
        public static bool UseStreaming = true;
        public static readonly string SpeedDescription = "When {{{{char}}}} wants to change the rate of the {0}. Higher numbers is a more intense {0}. The value 0 means stop the {0} where it currently is. It can be a number {1}-{2}.";
        public static readonly string RangeDescription = "The oscillation range of the {0}. It can be a number {1}-{2}. If the range is 0, the device channel will move to the position at the speed and stay until other wise requested.";
        public static readonly string PositionDescription = "The offset center position of the {0} range. This value will oscillate in the specified range if the speed is greater than 0. It can be a number {1}-{2}.";
        public static readonly string SpeedName = "{0}Speed";
        public static readonly string RangeName = "{0}Range";
        public static readonly string PositionName= "{0}Position";
        public static readonly int TCodeMin = 0;
        public static readonly int TCodeMax = 9999;
        public static readonly Channel Stroke = new()
        {
            Name = ChannelName.Stroke, 
            FullName = "stroke", 
            SpeedPercentage = Tuple.Create(0, 100),
            PositionPercentage = Tuple.Create(10, 90), 
            PositionDescription = "The offset of the {0} range. Where higher numbers is the head or tip of the penis and lower numbers is the base of the penis. It can be a number {1}-{2}. If the range is 0, the device channel will move to the position at the speed and stay until other wise requested.",
            RangePercentage = Tuple.Create(0, 100), 
            RangeDescription = "The oscillation range of the {0}. When {{{{char}}}} wants to stroke the penis of {{{{user}}}}. This value is how long the stroke is. Larger numbers, the longer the stroke. This value is relative to the position value. It can be a number {1}-{2}."
        };
        public static readonly Channel Surge = new()
        {
            Name = ChannelName.Surge, 
            FullName = "surge",
            SpeedPercentage = Tuple.Create(0, 100),
            PositionPercentage = Tuple.Create(10, 90),
            PositionDescription = "The offset center position of the {0} range. This value will oscillate in the specified range if the speed is greater than 0. The larger the number, the farther away from {{{{user}}}} the device will go. It can be a number {1}-{2}.",
            RangePercentage = Tuple.Create(0, 100),
            RangeDescription = "The oscillation range of the {0}. This value moves the stoker away and toward {{{{ user }}}} in a latteral motion. It can be a number {1}-{2}. If the range is 0, the device channel will move to the position at the speed and stay until other wise requested."
        };
        public static readonly Channel Sway = new()
        {
            Name = ChannelName.Sway, 
            FullName = "sway",
            SpeedPercentage = Tuple.Create(0, 100),
            PositionPercentage = Tuple.Create(10, 90),
            PositionDescription = "The offset center position of the {0} range. This value will oscillate in the specified range if the speed is greater than 0. The larger the number, the farther to the left of {{{{user}}}} the device will go. It can be a number {1}-{2}.",
            RangePercentage = Tuple.Create(0, 100),
            RangeDescription = "The oscillation range of the {0}. This value moves side to side in a latteral motion. It can be a number {1}-{2}. If the range is 0, the device channel will move to the position at the speed and stay until other wise requested."
        };
        public static readonly Channel Twist = new()
        {
            Name = ChannelName.Twist, 
            FullName = "twist",
            SpeedPercentage = Tuple.Create(0, 100),
            PositionPercentage = Tuple.Create(10, 90),
            RangePercentage = Tuple.Create(0, 100),
            RangeDescription = "The range of the {0}. This value moves around or yaw in a rotating motion. It can be a number {1}-{2}. If the range is 0, the device channel will move to the position at the speed and stay until other wise requested."
        };
        public static readonly Channel Roll = new()
        {
            Name = ChannelName.Roll, 
            FullName = "roll",
            SpeedPercentage = Tuple.Create(0, 100),
            PositionPercentage = Tuple.Create(10, 90),
            RangePercentage = Tuple.Create(0, 100),
            RangeDescription = "The range of the {0}. This value rocks side to side in a rotating motion. It can be a number {1}-{2}. If the range is 0, the device channel will move to the position at the speed and stay until other wise requested."
        };
        public static readonly Channel Pitch = new()
        {
            Name = ChannelName.Pitch, 
            FullName = "pitch",
            SpeedPercentage = Tuple.Create(0, 100),
            PositionPercentage = Tuple.Create(10, 90),
            RangePercentage = Tuple.Create(0, 100),
            RangeDescription = "The range of the {0}. This value rocks toward and away in a rotating motion. It can be a number {1}-{2}. If the range is 0, the device channel will move to the position at the speed and stay until other wise requested."
        };
        public static readonly Channel Suck = new()
        {
            Name = ChannelName.SuckLevel, 
            FullName = "suck",
            PositionPercentage =  Tuple.Create(0, 100)
        };        
        public static readonly Channel Lube = new()
        {
            Name = ChannelName.Lube, 
            FullName = "lube",
            PositionPercentage = Tuple.Create(0, 100),
            IsSwitch = true
        };
        public static readonly Channel Vibe1 = new()
        {
            Name = ChannelName.Vibe1, 
            FullName = "vibrator number 1",
            PositionPercentage = Tuple.Create(0, 100),
            PositionDescription = "The intensity of the first vibrator. It can be a number {1}-{2}. A value less than 30 turns the vibrator off", 
            IsSwitch = true
        };
        public static readonly Channel Vibe2 = new()
        {
            Name = ChannelName.Vibe2, 
            FullName = "vibrator number 2",
            PositionPercentage = Tuple.Create(0, 100),
            PositionDescription = "The intensity of the second vibrator. It can be a number {1}-{2}. A value less than 30 turns the vibrator off", 
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
        public Tuple<int, int>? SpeedPercentage {get;set;}
        public string RangeName {get { return string.Format(ChannelDefault.RangeName, FullName); }}
        public string  PositionName {get { return string.Format(ChannelDefault.PositionName, FullName); }}
        public string  SpeedName {get { return string.Format(ChannelDefault.SpeedName, FullName); }}
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
        public string SpeedDescription
        {
            get
            {
                if(m_speedDescription.Length == 0)
                    m_speedDescription = string.Format(ChannelDefault.SpeedDescription, FullName, SpeedPercentage?.Item1 ?? 0, SpeedPercentage?.Item2 ?? 100);
                return m_speedDescription;
            }
            set 
            { 
                m_speedDescription = string.Format(value, FullName, SpeedPercentage?.Item1 ?? 0, SpeedPercentage?.Item2 ?? 100);}
            }
        public bool IsSwitch = false;
        public bool Enabled = true;
        public ChannelTarget Target = new();
        public float Speed = 0;
        public float Top = 5000;
        public float Bottom = 5000;
        public float Timer = 0;
        public float LastTimer = 0;
        public bool AtTop = false;
        public int Min = 0;
        public int Max = 9999;
        public string LastTCode = "";
        private string m_rangeDescription = "";
        private string m_positionDescription = "";
        private string m_speedDescription = "";
        private readonly Mutex mutex = new();
        public bool Lock()
        {
            return false;//mutex.WaitOne();
        }
        public void Unlock()
        {
            //mutex.ReleaseMutex();
        }
    }
}