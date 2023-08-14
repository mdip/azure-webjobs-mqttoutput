using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Connection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Binding
{
    public static class MqttOutputBindingWebJobsBuilderExtension
    {
        public static IWebJobsBuilder AddMqttOutputBinding(this IWebJobsBuilder builder)
        {
            builder.Services.AddSingleton<IMqttConnectionFactory, MqttConnectionFactory>();

            builder.AddExtension<MqttOutputBindingExtensionConfigProvider>();
            return builder;
        }
    }
}
