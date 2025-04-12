# Voxta.TCode
TCode provider app for Voxta AI

## How to run
Modify the appsettings.json file and set your device settings.
* Wether or not to use a UDP connection.
    "UseUDP": true, 
* If serial is elected then this is the comport to use
    "SerialPort": "COM4",
* If UDP is selected then this is the address information
    "UDPAddress": "192.168.0.224",
    "UDPPort": 8000,
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
* This will reply in chat if you dont say anything within this timeout in milliseconds
    "AutoReplyDelay": 300000

If not installed, Install dotnet v9 then simply run RUN.bat OR use `dotnet run` and the app should start. It will connect to Voxta sessions on the current machine.

