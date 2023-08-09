
```
                  %%%%%%
                 %%%%%%
            @   %%%%%%    @
          @@   %%%%%%      @@
       @@@    %% MQTT %%    @@@
     @@      %%%%%%%%%%        @@
       @@         %%%%       @@
         @@      %%%       @@
           @@    %%      @@
                %%
                %

```

# Microsoft Azure WebJobs MQTT Output binding for Azure Functions
A WebJobs extension for MQTT output binding based on MQTTnet library and the Managed Client extension.

This project is based on https://github.com/keesschollaart81/CaseOnline.Azure.WebJobs.Extensions.Mqtt.

The repository contains the code for the Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding NuGet Package.
This package enables you to publish a message to a MQTT topic as a result of an Azure Function.

Are you curious what MQTT is? Check [this page](http://mqtt.org/faq)!

## How to use

### Getting Started

1) Create a custom configuration for the output binding by implementing the ```ICustomConfigurationProvider``` and defining your own MQTT client options. 
2) Use the output binding attribute ```[Mqtt]``` with the custom configuration passing its type to attribute. 
For example, if your configuration class is named ```MyCustomConfiguration``` the attribute should be used like this: ```[Mqtt(typeof(MyCustomConfiguration))]```.
3) In your azure function you'll be able to publish a new message with a fully custom configurable MQTT client.

### Custom Configuration Example
ClientOptions should not be null.

``` csharp
public class CustomCustomConfigurationProvider : ICustomConfigurationProvider
{
    private static readonly ManagedMqttClientOptions ManagedMqttClientOptions = BuildClientOptions();
    public ManagedMqttClientOptions ClientOptions => ManagedMqttClientOptions;

    private static ManagedMqttClientOptions BuildClientOptions()
    {
        ManagedMqttClientOptionsBuilder builder = new();
        MqttClientOptionsBuilder clientOptionsBuilder = new();
        clientOptionsBuilder
            .WithTcpServer("broker.hivemq.com",1883)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .WithClientId(Guid.NewGuid().ToString())
            .WithCredentials("user", "pass");
                
        builder
            .WithClientOptions(clientOptionsBuilder.Build())
            ;
        

        return builder.Build(); 
    }
}
```

### Publish via output

<!--
## Where to get

Install stable releases via Nuget; development releases are available via MyGet.

|                     | Master > NuGet | Dev > MyGet |
|--------------------------------|-----------------|-----------------|
| Build status |  [![Build Status](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_apis/build/status/CaseOnline.Azure.WebJobs.Extensions.Mqtt?branchName=master)](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_build/index?definitionId=11)   | [![Build Status](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_apis/build/status/CaseOnline.Azure.WebJobs.Extensions.Mqtt?branchName=dev)](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_build/index?definitionId=11)
| Deployment Status | [![Deployment Status](https://caseonline.vsrm.visualstudio.com/_apis/public/Release/badge/4df87c38-5691-4d04-8373-46c830209b7e/1/2)](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_releases2?definitionId=1)|[![Deployment Status](https://caseonline.vsrm.visualstudio.com/_apis/public/Release/badge/4df87c38-5691-4d04-8373-46c830209b7e/1/3)](https://caseonline.visualstudio.com/CaseOnline.Azure.WebJobs.Extensions.Mqtt/_releases2?definitionId=1)|
| Package | [![NuGet](https://img.shields.io/nuget/v/CaseOnline.Azure.WebJobs.Extensions.Mqtt.svg)](https://www.nuget.org/packages/CaseOnline.Azure.WebJobs.Extensions.Mqtt/) | [![MyGet](https://img.shields.io/myget/caseonline/v/CaseOnline.Azure.WebJobs.Extensions.Mqtt.svg)](https://www.myget.org/feed/caseonline/package/nuget/CaseOnline.Azure.WebJobs.Extensions.Mqtt) | 
-->

## Examples

This is a simple example, publishing messages on topic ```test/out```.

``` csharp
public static class Example
{
    [FunctionName("AsyncCollector")]
    public static async Task<IActionResult> RunAsyncCollector(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "async-collector")] HttpRequest req, 
        [Mqtt(typeof(CustomCustomConfigurationProvider))] IAsyncCollector<IMqttMessage> outMessages, 
        ILogger log)
    {

        await outMessages.AddAsync(
            new MqttMessage(topic: "test/out", message: Encoding.UTF8.GetBytes("hello"), qosLevel: MqttQualityOfServiceLevel.AtMostOnce, retain: false));

        return new OkObjectResult("Message Enqueued!");
    }
    
    [FunctionName("IMqttMessage")]
    public static IActionResult RunSingleMessage(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "mqtt-message")] HttpRequest req, 
        [Mqtt(typeof(CustomCustomConfigurationProvider))] out IMqttMessage outMessage,
        ILogger log)
    {
        outMessage = new MqttMessage(topic: "test/out", message: Encoding.UTF8.GetBytes("hello"), qosLevel: MqttQualityOfServiceLevel.AtMostOnce, retain: false);
        
        return new OkObjectResult("Message Enqueued!");
    }
    
}

```

Please find all working examples in the [sample project](./ExampleFunctions).


## References

- [MQTTnet](https://github.com/chkr1011/MQTTnet)


