# Voxta.TCode
TCode provider app for Voxta AI
﻿# Voxta Provider App Example

This example should get you started if you want to extend Voxta. Some typical examples:

- Listen to what the user or the AI says
- Register actions (character action inference)
- Assistant-like actions (user action inference)
- Update the context continuously
- Send messages on behalf of the user

## How to run
Modify the appsettings.json file and set your device settings.
The settings are as follows:
* Wether or not to use a UDP connection.
    "UseUDP": false, 
* If serial is elected then this is the comport to use
    "SerialPort": "COM4",
* If UDP is selected then this is the address information
    "UDPAddress": "tcode.local",
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

If not installed, Install dotnet v9 then simply execute RUN.bat OR use `dotnet run` and the app should start. It will connect to Voxta sessions on the current machine.

Simply use `dotnet run` and the app should start. It will connect to Voxta sessions on the current machine.

## How to make it yours

In `Program.cs`, register your own providers. For example:

```csharp
// Voxta Providers
services.AddVoxtaProvider(builder =>
{
    // Add the providers you want
    builder.AddProvider<MyCustomProvider>();
});
```

You can always check out the examples in the `Providers` folder. Here's the basic structure of a provider:

```csharp
public class MyCustomProvider : ProviderBase
{
    // This uses dependency injection, feel free to add more dependencies
    public ActionProvider(IRemoteChatSession session, ILogger<ActionProvider> logger)
        : base(session, logger)
    {
    }
    
    protected override async Task OnStartAsync()
    {
        await base.OnStartAsync();
        // Here you can register events and initialize your provider
        // See the examples for more information
    }
}
```