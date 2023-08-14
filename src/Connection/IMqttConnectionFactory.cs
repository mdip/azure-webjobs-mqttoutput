using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Binding;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Connection;

namespace Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Abstractions
{
    public interface IMqttConnectionFactory
    {
        MqttConnection GetConnection(MqttAttribute mqttAttribute);
    }
}