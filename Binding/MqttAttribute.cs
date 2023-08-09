using System;
using Microsoft.Azure.WebJobs.Description;
using MQTTnet.Extensions.ManagedClient;

namespace Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Binding
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public class MqttAttribute : Attribute
    {
        public MqttAttribute(Type clientOptionsType)
        {
            ClientOptionsType = clientOptionsType;
            ClientOptions = ((ICustomConfigurationProvider)Activator.CreateInstance(ClientOptionsType)).ClientOptions;
        }

        public Type ClientOptionsType { get; }

        public ManagedMqttClientOptions ClientOptions { get; }
    }

    public interface ICustomConfigurationProvider
    {
        public ManagedMqttClientOptions ClientOptions { get; }
    }
}