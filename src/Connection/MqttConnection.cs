using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Exceptions;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Connection
{
    public sealed class MqttConnection : IDisposable, IAsyncDisposable
    {
        private readonly ManagedMqttClientOptions _config;
        private readonly ILogger<MqttConnection> _logger;
        private readonly object _startupLock = new();
        private IManagedMqttClient? _managedMqttClient;

        public MqttConnection(ManagedMqttClientOptions config, ILogger<MqttConnection> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current status of the connection.
        /// </summary>
        public ConnectionState ConnectionState { get; private set; }

        /// <summary>
        /// Gets the descriptor for this Connection.
        /// </summary>
        public override string ToString()
        {
            return $"Connection for config: {_config}, currently connected: {ConnectionState}";
        }
        
        /// <summary>
        /// Opens the MQTT connection.
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                lock (_startupLock)
                {
                    if (_managedMqttClient != null || ConnectionState == ConnectionState.Connected)
                    {
                        return;
                    }

                    ConnectionState = ConnectionState.Connecting;
                    _managedMqttClient = new MqttFactory().CreateManagedMqttClient();
                    _managedMqttClient.ConnectedAsync += async e => { await HandleConnectedAsync(e); };
                    _managedMqttClient.ConnectingFailedAsync += async e => { await HandleConnectingFailedAsync(e); };
                    _managedMqttClient.ApplicationMessageProcessedAsync += async e => { await HandleApplicationMessageProcessedAsync(e); };
                    _managedMqttClient.DisconnectedAsync += async e => { await HandleDisconnectedAsync(e); };
                }

                await _managedMqttClient.StartAsync(_config);
            }
            catch (Exception e)
            {
                _logger.LogCritical(new EventId(0), e, "Exception while setting up the mqtt client to {Unknown}", this);
                throw new MqttConnectionException($"Exception while setting up the mqtt client to {this}", e);
            }
        }

        private Task HandleApplicationMessageProcessedAsync(ApplicationMessageProcessedEventArgs eventArgs)
        {
            if (eventArgs.Exception is not null)
            {
                _logger.LogError(new EventId(0), eventArgs.Exception, "Message could not be processed for {Unknown}, message: {ExceptionMessage}", this, eventArgs.Exception?.Message);
            }

            return Task.CompletedTask;
        }

        private Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            ConnectionState = ConnectionState.Disconnected;
            _logger.LogWarning(new EventId(0), eventArgs.Exception,
                "MqttConnection Disconnected, previous connectivity state {EventArgsClientWasConnected} for {Unknown}, message: {ExceptionMessage}",
                eventArgs.ClientWasConnected, this, eventArgs.Exception?.Message);
            
            return Task.CompletedTask;
        }

        private Task HandleConnectingFailedAsync(ConnectingFailedEventArgs eventArgs)
        {
            ConnectionState = ConnectionState.Disconnected;
            _logger.LogWarning(new EventId(0), eventArgs.Exception, "MqttConnection could not connect for {Unknown}, message: '{ExceptionMessage}", this, eventArgs.Exception?.Message);
            
            return Task.CompletedTask;
        }
        
        private Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            if (eventArgs.ConnectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                ConnectionState = ConnectionState.Connected;
                _logger.LogInformation("MqttConnection Connected for {Unknown}", this);
            }
            else
            {
                ConnectionState = ConnectionState.Disconnected;
                _logger.LogWarning("MqttConnection could not connect, result code: {ConnectResultResultCode} for {Unknown}", eventArgs.ConnectResult.ResultCode, this);
            }
            return Task.CompletedTask;
        }

        public async Task PublishAsync(MqttApplicationMessage message)
        {
            if (_managedMqttClient is null)
            {
                throw new MqttConnectionException("Connection not open, please use StartAsync first!");
            }

            await _managedMqttClient.EnqueueAsync(message);

            _logger.LogDebug("Pending messages after enqueue: {PendingApplicationMessagesCount}", _managedMqttClient.PendingApplicationMessagesCount);
        }

        /// <summary>
        /// Close the MQTT connection.
        /// </summary>
        public Task StopAsync()
        {
            if (_managedMqttClient is not null)
            {
                Dispose();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_managedMqttClient is null) return;
            
            ConnectionState = ConnectionState.Disconnected;
            
            _managedMqttClient.Dispose();

            _managedMqttClient = null;
        }
        
        public async ValueTask DisposeAsync()
        {
            if (_managedMqttClient is not null)
            {
                await _managedMqttClient.StopAsync();
                
                Dispose();
            }
        }
    }
}
