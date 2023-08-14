using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Abstractions;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Binding
{
    /// <summary>
    /// Registers the <see cref="MqttAttribute"/> binding.
    /// </summary>
    [Extension("Mqtt")]
    public class MqttOutputBindingExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMqttConnectionFactory _connectionFactory;

        public MqttOutputBindingExtensionConfigProvider(IMqttConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Initializes the extension configuration provider.
        /// </summary>
        /// <param name="context">The extension configuration context.</param>
        public void Initialize(ExtensionConfigContext context)
        {
            var mqttAttributeBindingRule = context.AddBindingRule<MqttAttribute>();
            mqttAttributeBindingRule.BindToCollector((attr) => new MqttMessageCollector(_connectionFactory.GetConnection(attr), _loggerFactory.CreateLogger<MqttMessageCollector>()));
        }
    }
}
