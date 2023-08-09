
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
A WebJobs extension for MQTT output binding based on MQTTNet library and the Managed Client extension.

This project is based on https://github.com/keesschollaart81/CaseOnline.Azure.WebJobs.Extensions.Mqtt.

This repository contains the code for the Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding NuGet Package.

This package enables you to:

* Publish a message to a MQTT topic as a result of an Azure Function

Are you curious what MQTT is? Check [this page](http://mqtt.org/faq)!

## How to use

<!--
* [Getting Started](/../../wiki/Getting-started)
* [Publish via output](/../../wiki/Publish-via-output)
* [Subscribe via trigger](/../../wiki/Subscribe-via-trigger)
* [Integrate with Azure IoT Hub](/../../wiki/Azure-IoT-Hub)
* [And more in the Wiki](/../../wiki)
-->

### Getting Started
To start you first need to define a configuration for the output binding.
Implement the ICustomConfigurationProvider.

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

This is a simple example, publishing messages on topic ```testtopic/out```.

``` csharp
public static class Example
{
    [FunctionName("AsyncCollector")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "async-collector")] HttpRequest req, 
        [Mqtt(typeof(CustomCustomConfigurationProvider))] IAsyncCollector<IMqttMessage> outMessages, 
        ILogger log)
    {

        await outMessages.AddAsync(
            new MqttMessage(topic: "test", message: Encoding.UTF8.GetBytes("hello"), qosLevel: MqttQualityOfServiceLevel.AtMostOnce, retain: false));

        return new OkObjectResult("Message Enqueued!");
    }
    
    [FunctionName("IMqttMessage")]
    public static IActionResult Run1(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "mqtt-message")] HttpRequest req, 
        [Mqtt(typeof(CustomCustomConfigurationProvider))] out IMqttMessage outMessage,
        ILogger log)
    {
        outMessage = new MqttMessage(topic: "test", message: Encoding.UTF8.GetBytes("hello"), qosLevel: MqttQualityOfServiceLevel.AtMostOnce, retain: false);
        
        return new OkObjectResult("Message Enqueued!");
    }
    
}

```

<!-- Please find all working examples in the [sample project](./src/ExampleFunctions/). -->


## References

- [MQTTnet](https://github.com/chkr1011/MQTTnet)


