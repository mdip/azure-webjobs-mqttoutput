using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Binding;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace MqttOutputFunctions;

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

public class CustomCustomConfigurationProvider : ICustomConfigurationProvider
{
    public ManagedMqttClientOptions ClientOptions
    {
        get
        {
            ManagedMqttClientOptionsBuilder builder = new();
            MqttClientOptionsBuilder clientOptionsBuilder = new();
            clientOptionsBuilder
                .WithTcpServer("server",1883)
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .WithClientId("test")
                .WithCredentials("user", "pass");
                
            builder
                .WithClientOptions(clientOptionsBuilder.Build())
                ;
        

            return builder.Build();
        }
    }
}
