using MockWebApi.Configuration.Model;
using System.Linq;

namespace MockWebApi.Configuration
{
    public class ServiceConfigurationWriter : IServiceConfigurationWriter
    {

        private readonly IConfigurationFileWriter _configurationWriter;
        private readonly IServiceConfiguration _serviceConfiguration;

        public ServiceConfigurationWriter(
            IConfigurationFileWriter configurationWriter,
            IServiceConfiguration serviceConfiguration)
        {
            _configurationWriter = configurationWriter;
            _serviceConfiguration = serviceConfiguration;
        }

        public string WriteConfiguration(MockedServiceConfiguration serviceConfiguration, string outputFormat = "YAML")
        {
            return _configurationWriter.WriteConfiguration(serviceConfiguration, outputFormat.ToUpper());
        }

        public MockedServiceConfiguration GetServiceConfiguration()
        {
            MockedServiceConfiguration serviceConfiguration = new MockedServiceConfiguration
            {
                ServiceName = _serviceConfiguration.ServiceName,
                BaseUrl = _serviceConfiguration.Url,
                DefaultEndpointDescription = _serviceConfiguration.DefaultEndpointDescription,
                JwtServiceOptions = _serviceConfiguration.JwtServiceOptions,
                EndpointDescriptions = _serviceConfiguration.RouteMatcher
                    .GetAllRoutes()
                    .Select(s => s.EndpointDescription)
                    .ToArray()
            };

            return serviceConfiguration;
        }

    }
}
