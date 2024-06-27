using MockWebApi.Configuration.Model;
using System.Linq;

namespace MockWebApi.Configuration
{
    public class ServiceConfigurationWriter : IServiceConfigurationWriter
    {

        public ServiceConfigurationWriter(
            IConfigurationWriter configurationWriter,
            IRestServiceConfiguration serviceConfiguration)
        {
            _configurationWriter = configurationWriter;
            _serviceConfiguration = serviceConfiguration;
        }

        public static string WriteConfiguration(IConfigurationWriter writer, IServiceConfiguration configuration, string outputFormat = "YAML")
        {
            //TODO: implement this. This needs a more diverse set of configuration classes in
            // the MockWebApi.Configuration library.
            return string.Empty;
        }

        public string WriteConfiguration(MockedRestServiceConfiguration serviceConfiguration, string outputFormat = "YAML")
        {
            return _configurationWriter.WriteConfiguration(serviceConfiguration, outputFormat.ToUpper());
        }

        public MockedRestServiceConfiguration GetServiceConfiguration()
        {
            MockedRestServiceConfiguration serviceConfiguration = new MockedRestServiceConfiguration
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


        private readonly IConfigurationWriter _configurationWriter;
        private readonly IRestServiceConfiguration _serviceConfiguration;


    }
}
