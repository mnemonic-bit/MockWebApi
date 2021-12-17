using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public class HostConfigurationWriter : IHostConfigurationWriter
    {

        private readonly IConfigurationFileWriter _configurationWriter;
        private readonly IHostConfiguration _hostConfiguration;

        public HostConfigurationWriter(
            IConfigurationFileWriter configurationWriter,
            IHostConfiguration hostConfiguration)
        {
            _configurationWriter = configurationWriter;
            _hostConfiguration = hostConfiguration;
        }

        public string WriteConfiguration(MockedHostConfiguration serviceConfiguration, string outputFormat = "YAML")
        {
            return _configurationWriter.WriteConfiguration(serviceConfiguration, outputFormat.ToUpper());
        }

        public MockedHostConfiguration GetHostConfiguration()
        {
            MockedHostConfiguration serviceConfiguration = new MockedHostConfiguration
            {
                TrackServiceApiCalls = _hostConfiguration.TrackServiceApiCalls,
                LogServiceApiCalls = _hostConfiguration.LogServiceApiCalls,

                Services = null
            };

            return serviceConfiguration;
        }

    }
}
