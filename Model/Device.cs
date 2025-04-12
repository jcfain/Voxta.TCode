namespace Voxta.TCode.Model
{
    public enum DeviceType
    {
        SSR1,
        OSR2,
        OSR6
    }

    public class Device
    {
        public readonly DeviceType type;
        public readonly Dictionary<ChannelID, Channel> ChannelsMap;
        public Device(DeviceType type)
        {
            this.type = type;
            switch (type)
            {
                case DeviceType.SSR1:
                    {
                        ChannelsMap = new()
                    {
                        { ChannelID.Stroke, ChannelDefault.Stroke },
                        { ChannelID.Twist, ChannelDefault.Twist }
                    };
                    }
                    break;
                case DeviceType.OSR2:
                    {
                        ChannelsMap = new()
                    {
                        { ChannelID.Stroke, ChannelDefault.Stroke },
                        { ChannelID.Twist, ChannelDefault.Twist },
                        { ChannelID.Roll, ChannelDefault.Roll },
                        { ChannelID.Pitch, ChannelDefault.Pitch }
                    };
                    }
                    break;
                default:
                    ChannelsMap = new()
                    {
                        { ChannelID.Stroke, ChannelDefault.Stroke },
                        { ChannelID.Surge, ChannelDefault.Surge },
                        { ChannelID.Sway, ChannelDefault.Sway },
                        { ChannelID.Twist, ChannelDefault.Twist },
                        { ChannelID.Roll, ChannelDefault.Roll },
                        { ChannelID.Pitch, ChannelDefault.Pitch }
                    };
                    break;
            }
            ChannelsMap.Add(ChannelID.SuckLevel, ChannelDefault.Suck);
            ChannelsMap.Add(ChannelID.Lube, ChannelDefault.Lube);
            ChannelsMap.Add(ChannelID.Vibe1, ChannelDefault.Vibe1);
            ChannelsMap.Add(ChannelID.Vibe2, ChannelDefault.Vibe2);
        }
    }
}