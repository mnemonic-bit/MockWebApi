using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IServiceConfigurationWriter
    {

        public string WriteConfiguration(MockedWebApiServiceConfiguration serviceConfiguration, string outputFormat = "YAML");

        public MockedWebApiServiceConfiguration GetServiceConfiguration();

    }
}
