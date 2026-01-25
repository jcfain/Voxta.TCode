# Voxta.TCode
TCode provider app for Voxta AI

## How to run
Modify the appsettings.json file and set your device settings.
The settings are as follows:
* ConnectionType is the connection you will be using. The enum map is as follows.
    Serial: 0
    UDP: 1
    Websocket: 2
* If serial is elected then this is the comport to use
    "SerialPort": "COM4",
* If UDP is selected then this is the address information
    "NetworkAddress": "tcode.local",
    "NetworkPort": 8000,
* DeviceType is the device you will be using. The enum map is as follows.
    SSR1: 0
    OSR2: 1
    OSR6: 2
* Channel enabled.
    "(Channel)Enabled": Can disable the channel. Note: if you set the device type to a device that doesnt have the channel, then this has no effect,
* The absolute mins and maximums tailored to your needs. 
    (Only some devices have the Channels. You only need to set the ones you need for your DeviceType specified above)
    "(Channel)Min": 0,
    "(Channel)Max": 9999
* This will reply in chat if you dont say anything within this timeout in milliseconds -1 to disable
    "AutoReplyDelay": 300000
* This will control the min and max speed of the channels in milliseconds. ex: MaxInterval: 5000 takes 5 seconds for half an occilation 
    "MaxInterval": 2500,
    "MinInterval": 300

If not installed, Install dotnet v10 then simply execute RUN.bat OR use `dotnet run` and the app should start. It will connect to Voxta sessions on the current machine.

Simply use `dotnet run` and the app should start. It will connect to Voxta sessions on the current machine.