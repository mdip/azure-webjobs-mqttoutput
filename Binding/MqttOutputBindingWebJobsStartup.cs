using Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Binding;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(MqttOutputBindingWebJobsStartup))]
namespace Microsoft.Azure.WebJobs.Extensions.MqttOutputBinding.Binding
{
    public class MqttOutputBindingWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddMqttOutputBinding();
        }
    }
}
