using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IHostConfigurationWriter
    {

        public string WriteConfiguration(MockedHostConfiguration hostConfiguration, string outputFormat = "YAML");

        public MockedHostConfiguration GetHostConfiguration();

    }
}
