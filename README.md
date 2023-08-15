
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

The repository contains the code for the WebJobs.Extensions.MqttOutputBinding NuGet Package.
This package enables you to publish a message to a MQTT topic as a result of an Azure Function.

Are you curious what MQTT is? Check [this page](http://mqtt.org/faq)!

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/mdip/azure-webjobs-mqttoutput/blob/main/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/WebJobs.Extensions.MQTT.OutputBinding.svg)](https://www.nuget.org/packages/WebJobs.Extensions.MQTT.OutputBinding/)

## How to use

### Getting Started

1) Create a custom configuration for the output binding by implementing the ```ICustomConfigurationProvider``` and defining your own MQTT client options.
2) Use the output binding attribute ```[Mqtt]``` with the custom configuration passing its type to attribute.
   For example, if your configuration class is named ```MyCustomConfiguration``` the attribute usage should be used like this: ```[Mqtt(typeof(MyCustomConfiguration))]```.
3) In your azure function you'll be able to publish a new message with a fully custom configurable MQTT client. See the examples for more.

### Custom Configuration Example
```ClientOptions``` property must not be null. The following example shows how to create a custom configuration.
In this example a private static property has been used in order to build the configuration only once.

``` csharp
public class CustomCustomConfigurationProvider : ICustomConfigurationProvider
{
    private static readonly ManagedMqttClientOptions _managedMqttClientOptions = BuildClientOptions();
    public ManagedMqttClientOptions ClientOptions => _managedMqttClientOptions;

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

### Publish with output binding examples

Publishing messages on topic ```test/out```.

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

Please, see the examples in the [sample project](./ExampleFunctions).


## References

- [MQTTnet](https://github.com/chkr1011/MQTTnet)
- [WebJobs custom input and output bindings](https://github.com/Azure/azure-webjobs-sdk/wiki/Creating-custom-input-and-output-bindings)
- [Custom bindings with isolated worker](https://blog.maartenballiauw.be/post/2021/06/01/custom-bindings-with-azure-functions-dotnet-isolated-worker.html)
- [Public MQTT Broker for tests](https://www.hivemq.com/public-mqtt-broker/)
