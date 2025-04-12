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
                        { ChannelID.Stroke, new Channel(ChannelName.Stroke, "Stroke", 
                            "The range of the stroke. It can be a number 10-100.", 
                            "The position of the stroke range. Where 90 is the head or tip of the cock, 10 is the base of the cock and 50 is the middle of the cock. It can be a number 10-90.", 
                            "The intensity of the strokes. It can be a number 1-10.")},
                        { ChannelID.Twist, new Channel(ChannelName.Twist, "twisting")}
                    };
                    }
                    break;
                case DeviceType.OSR2:
                    {
                        ChannelsMap = new()
                    {
                        { ChannelID.Stroke, new Channel(ChannelName.Stroke, "Stroke")},
                        { ChannelID.Twist, new Channel(ChannelName.Twist, "twisting")},
                        { ChannelID.Roll, new Channel(ChannelName.Roll, "roll")},
                        { ChannelID.Pitch, new Channel(ChannelName.Pitch, "pitch")}
                    };
                    }
                    break;
                default:
                    ChannelsMap = new()
                    {
                        { ChannelID.Stroke, new Channel(ChannelName.Stroke, "Stroke")},
                        { ChannelID.Surge, new Channel(ChannelName.Surge, "surge")},
                        { ChannelID.Sway, new Channel(ChannelName.Sway, "sway")},
                        { ChannelID.Twist, new Channel(ChannelName.Twist, "twisting")},
                        { ChannelID.Roll, new Channel(ChannelName.Roll, "roll")},
                        { ChannelID.Pitch, new Channel(ChannelName.Pitch, "pitch")}
                    };
                    break;
            }
            ChannelsMap.Add(ChannelID.SuckLevel, new Channel(ChannelName.SuckLevel, "Suck", 
                            "", 
                            "", 
                            "How much suction to generate. It can be a number 0-10."));
            // ChannelsMap.Add(ChannelID.SuckManual, new Channel(ChannelName.SuckManual, "SuckManual", 
            //                 "", 
            //                 "", 
            //                 "How much suction to generate. It can be a number 0-10."));
            ChannelsMap.Add(ChannelID.Lube, new Channel(ChannelName.Lube, "Lube", 
                            "", 
                            "How much lube to coat the cock with. It can be a number 0-10.", 
                            ""));
            ChannelsMap.Add(ChannelID.Vibe1, new Channel(ChannelName.Vibe1, "Vibrator number 1", 
                            "", 
                            "The intensity of the first vibrator. It can be a number 0-10.", 
                            "", true));
            ChannelsMap.Add(ChannelID.Vibe2, new Channel(ChannelName.Vibe2, "Viberator number 2", 
                            "", 
                            "The intensity of the second vibrator. It can be a number 0-10.", 
                            "", true));
        }
    }
}