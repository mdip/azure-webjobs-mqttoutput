using System.Text;
using AzureFunctions.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace OutputBindingTests
{
    public class MqttOutputBindingTests
    {
        private const string  Topic = "0x3ff/test/out";
        private const string PayloadMessage = "TestMessage";
        
        [Fact]
        public async Task SimpleMessageIsPublished()
        {
            var receivedMessage = string.Empty;
            
            // Arrange
            using (var host = new HostBuilder()
                       .ConfigureWebJobs(builder => builder
                           .AddMqttOutputBinding()
                           .AddHttp())
                       .Build())
            {
                await host.StartAsync();
                var jobs = host.Services.GetService<IJobHost>();
                
                
                var mqttFactory = new MqttFactory();
                
                using (var managedMqttClient = mqttFactory.CreateManagedMqttClient())
                {
                    managedMqttClient.ApplicationMessageReceivedAsync += e =>
                    {
                        receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.ToArray());
                        return Task.CompletedTask;
                    };

                    await managedMqttClient.StartAsync(new MqttCustomConfigurationProvider().ClientOptions);

                    await managedMqttClient.SubscribeAsync(Topic);
                    
                    // Act
                    await jobs.CallAsync(nameof(MqttSimpleOutputIsPublishedTestFunction), new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest()
                    });

                    // Wait some seconds to consume the incoming message before disposing the client
                    await Task.Delay(10000);
                }
            }

            Assert.Equal(PayloadMessage, receivedMessage);
            
        }
    }
    
    public class MqttCustomConfigurationProvider : ICustomConfigurationProvider
    {
        private static readonly ManagedMqttClientOptions ManagedMqttClientOptions = BuildClientOptions();
        public ManagedMqttClientOptions ClientOptions => ManagedMqttClientOptions;

        private static ManagedMqttClientOptions BuildClientOptions()
        {
            ManagedMqttClientOptionsBuilder builder = new();
            MqttClientOptionsBuilder clientOptionsBuilder = new();
            clientOptionsBuilder
                .WithTcpServer("broker.hivemq.com", 1883)
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .WithClientId($"test-{Guid.NewGuid()}");
                
            builder.WithClientOptions(clientOptionsBuilder.Build());
            
            return builder.Build(); 
        }
    }
    
    public static class MqttSimpleOutputIsPublishedTestFunction
    {
        private const string  Topic = "0x3ff/test/out";
        private const string PayloadMessage = "TestMessage";
        
        [FunctionName(nameof(MqttSimpleOutputIsPublishedTestFunction))]

        public static IActionResult RunTest(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Mqtt(typeof(MqttCustomConfigurationProvider))] out IMqttMessage mqttMessage)
        {
            mqttMessage = new MqttMessage(Topic, new ArraySegment<byte>(Encoding.UTF8.GetBytes(PayloadMessage)), MqttQualityOfServiceLevel.ExactlyOnce, true);

            return new OkResult();
        }
    }
    

    
}
