using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IServiceConfigurationWriter
    {

        public string WriteConfiguration(MockedServiceConfiguration serviceConfiguration, string outputFormat = "YAML");

        public MockedServiceConfiguration GetServiceConfiguration();

    }
}
