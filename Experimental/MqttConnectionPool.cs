using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Connection;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Exceptions;
using Microsoft.Extensions.Logging;
using MQTTnet.Extensions.ManagedClient;

namespace Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Experimental
{
    /// <summary>
    /// Note: this is an experimental class that should not be used at the moment.
    /// </summary>
    public sealed class MqttConnectionPool : IDisposable
    {
        private readonly ConcurrentDictionary<long, MqttConnection> _connections = new();
        private readonly int _maxConnections;
        private long _counter;

        public MqttConnectionPool(ManagedMqttClientOptions configuration, ILogger<MqttConnection> logger)
        {
            _maxConnections = Environment.ProcessorCount;

            for (int connection = 0; connection < _maxConnections; connection++)
            {
                if (!_connections.TryAdd(connection, new MqttConnection(configuration, logger)))
                {
                    throw new MqttConnectionException("Unable to add the connection to the pool");
                }
            }
        }

        public void Dispose()
        {
            foreach (var connection in _connections.Values)
            {
                connection.Dispose();
            }
        }

        public MqttConnection GetConnection()
        {
            if (_connections.TryGetValue(_counter, out var connection))
            {
                Interlocked.Add(ref _counter, 1);

                if (Interlocked.Read(ref _counter) > _maxConnections - 1)
                {
                    Interlocked.Exchange(ref _counter, 0);
                }

                return connection;
            }

            throw new MqttConnectionException("No connection found");
        }
    }
}
