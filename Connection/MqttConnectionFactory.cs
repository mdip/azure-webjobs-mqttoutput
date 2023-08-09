using System;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Binding;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Connection
{
    public sealed class MqttConnectionFactory : IMqttConnectionFactory, IDisposable
    {
        private readonly ILoggerFactory _logger;
        private MqttConnection? _connection;
        private readonly object _sync = new();


        public MqttConnectionFactory(ILoggerFactory logger)
        {
            _logger = logger;
        }

        public MqttConnection GetConnection(MqttAttribute mqttAttribute)
        {
            lock (_sync)
            {
                _connection ??= new MqttConnection(mqttAttribute.ClientOptions, _logger.CreateLogger<MqttConnection>());
            }

            return _connection;
        }

        public void Dispose()
        {
            _logger.Dispose();
            _connection?.Dispose();
        }
    }
}
