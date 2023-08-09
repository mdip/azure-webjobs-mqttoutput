using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Connection;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Binding
{
    public class MqttMessageCollector : IAsyncCollector<IMqttMessage>
    {
        private readonly ILogger<MqttMessageCollector> _logger;
        private readonly MqttConnection _connection;

        public MqttMessageCollector(MqttConnection connection, ILogger<MqttMessageCollector> logger)
        {
            _logger = logger;
            _connection = connection;
        }

        public async Task AddAsync(IMqttMessage item, CancellationToken cancellationToken = default)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (_connection.ConnectionState != ConnectionState.Connected)
            {
                await _connection.StartAsync();
            }

            var mqttApplicationMessage = new MqttApplicationMessage
            {
                Topic = item.Topic,
                PayloadSegment = item.Message,
                QualityOfServiceLevel = item.QosLevel,
                Retain = item.Retain
            };

            await _connection.PublishAsync(mqttApplicationMessage);

            _logger.LogDebug("Publish message to topic '{ItemTopic}' requested", item.Topic);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
