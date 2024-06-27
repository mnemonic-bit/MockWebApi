using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IServiceConfigurationWriter
    {

        public string WriteConfiguration(MockedRestServiceConfiguration serviceConfiguration, string outputFormat = "YAML");

        public MockedRestServiceConfiguration GetServiceConfiguration();

    }
}
