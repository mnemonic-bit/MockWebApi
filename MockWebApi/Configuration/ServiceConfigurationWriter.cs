using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using System.Linq;

namespace MockWebApi.Configuration
{
    public class ServiceConfigurationWriter : IServiceConfigurationWriter
    {

        private readonly IConfigurationWriter _configurationWriter;
        private readonly IServiceConfiguration _serviceConfiguration;

        public ServiceConfigurationWriter(
            IConfigurationWriter configurationWriter,
            IServiceConfiguration serviceConfiguration)
        {
            _configurationWriter = configurationWriter;
            _serviceConfiguration = serviceConfiguration;
        }

        public string WriteConfiguration(MockedWebApiServiceConfiguration serviceConfiguration, string outputFormat = "YAML")
        {
            switch (outputFormat.ToUpper())
            {
                case "JSON":
                    {
                        return _configurationWriter.WriteToJson(serviceConfiguration);
                    }
                case "CS":
                    {
                        return "This format is not implemented yet.";
                    }
                case "YAML":
                default:
                    {
                        return _configurationWriter.WriteToYaml(serviceConfiguration);
                    }
            }
        }

        public MockedWebApiServiceConfiguration GetServiceConfiguration()
        {
            MockedWebApiServiceConfiguration serviceConfiguration = new MockedWebApiServiceConfiguration
            {
                //TODO: make the getters accept nullable types (e.g. 'bool?')
                TrackServiceApiCalls = _serviceConfiguration.ConfigurationCollection.Get<bool>(ConfigurationCollection.Parameters.TrackServiceApiCalls),
                LogServiceApiCalls = _serviceConfiguration.ConfigurationCollection.Get<bool>(ConfigurationCollection.Parameters.LogServiceApiCalls),
                //TODO: implement converting default endpoint definition
                //DefaultHttpStatusCode = _serviceConfiguration.Get<int>(ConfigurationCollection.Parameters.DefaultHttpStatusCode),
                //DefaultContentType = _serviceConfiguration.Get<string>(ConfigurationCollection.Parameters.DefaultContentType),

                EndpointDescriptions = _serviceConfiguration.RouteMatcher.GetAllRoutes().ToArray()
            };

            return serviceConfiguration;
        }

    }
}
