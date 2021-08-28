using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IServiceConfigurationWriter
    {

        public string WriteConfiguration(ServiceConfiguration serviceConfiguration, string outputFormat = "YAML");

        public ServiceConfiguration GetServiceConfiguration();

    }
}
