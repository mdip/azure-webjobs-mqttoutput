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

                    await managedMqttClient.StartAsync(new MqttCustomCustomConfigurationProvider().ClientOptions);

                    await Task.Delay(3000);

                    await managedMqttClient.SubscribeAsync(Topic);
                    
                    // Act
                    await jobs.CallAsync(nameof(MqttSimpleOutputIsPublishedTestFunction), new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest()
                    });
                    
                }
            }

            await Task.Delay(5000);

            Assert.Equal(PayloadMessage, receivedMessage);
            
        }
    }
    
    public class MqttCustomCustomConfigurationProvider : ICustomConfigurationProvider
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
                .WithClientId($"test-{Guid.NewGuid().ToString()}");
                
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
            [Mqtt(typeof(MqttCustomCustomConfigurationProvider))] out IMqttMessage mqttMessage)
        {
            mqttMessage = new MqttMessage(Topic, new ArraySegment<byte>(Encoding.UTF8.GetBytes(PayloadMessage)), MqttQualityOfServiceLevel.AtLeastOnce, true);

            return new OkResult();
        }
    }
    

    
}
