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
* The absolute mins and maximums tailored to your needs
    "StrokeMin": 0,
    "StrokeMax": 9999
* This will reply in chat if you dont say anything within this timeout in milliseconds
    "AutoReplyDelay": 300000

If not installed, Install dotnet v9 then simply run RUN.bat OR use `dotnet run` and the app should start. It will connect to Voxta sessions on the current machine.

