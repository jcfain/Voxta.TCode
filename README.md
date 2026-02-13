# Voxta.TCode
TCode provider app for Voxta AI

## How to run
Modify the appsettings.json file and set your device settings.
You can have multiple devices with different settings.
The default device is specified in the setting: "SelectedDevice"

The settings are as follows:
* This will reply in chat if you dont say anything within this timeout in milliseconds -1 to disable
    "AutoReplyDelay": 300000
* SelectedDevice is the default device on startup that will be connected to.
* Devices is a list of possible devices to connect to. Two examples are provieded. Feel free to delete one and modify as needed. 
Each device has the following properties:
  * ConnectionType is the connection you will be using. The enum map is as follows.
    * Serial: 0
    * UDP: 1
    * Websocket: 2
  * If serial is elected then this is the comport to use
    * "SerialPort": "COM4",
  * If UDP is selected then this is the address information
    * "NetworkAddress": "tcode.local",
    * "NetworkPort": 8000,
  * DeviceType is the device you will be using. The enum map is as follows.
    * SSR1:  0
    * SSR2:  1
    * OSR2:  2
    * OSR6:  3
    * TVIBE: 4
  * Channel enabled.
    * "(Channel)Enabled": Can disable the channel. IMPORTANT: if you set the device type to a device that doesnt natively have the channel (ex: OSR2 has no sway), then this has no effect,
  * The absolute mins and maximums tailored to your needs. 
    (Only some devices have the Channels. You only need to set the ones you need for your DeviceType specified above)
    * "(Channel)Min": 0,
    * "(Channel)Max": 9999
  * This will control the min and max speed of the channels in milliseconds. ex: MaxInterval: 5000 takes 5 seconds for half an oscillation 
    * "MaxInterval": 2500,
    * "MinInterval": 300

If not installed, Install dotnet v10 then simply execute RUN.bat OR use `dotnet run` and the app should start. It will connect to Voxta sessions on the current machine.

Simply use `dotnet run` and the app should start. It will connect to Voxta sessions on the current machine.

Things todo still:
Theres no much of a UI. You can execute a few things like connect and disconnect from the inspector (see below). A lack of UI may not be a huge deal to you but for running as a service its kind of needed.
You can make the initial configuration before starting the service but, theres things that would be allot easier to change with a GUI.

IMPORTANT: In the chat, pull up the Inspector. You can manually execute some commands from the actions section.
If setup with the correct device, you shouldnt have to do this. But its good to know its there just in case.

Also, this runs on linux pretty well :)
