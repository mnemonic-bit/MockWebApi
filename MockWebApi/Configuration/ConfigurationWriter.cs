using MockWebApi.Data;
using MockWebApi.Model;
using MockWebApi.Routing;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace MockWebApi.Configuration
{
    public class ConfigurationWriter : IConfigurationWriter
    {

        private readonly IConfigurationCollection _serviceConfiguration;
        private readonly IRouteMatcher<EndpointDescription> _routeMatcher;

        public ConfigurationWriter(
            IConfigurationCollection serviceConfiguration,
            IRouteMatcher<EndpointDescription> routeMatcher)
        {
            _serviceConfiguration = serviceConfiguration;
            _routeMatcher = routeMatcher;
        }

        public string WriteConfiguration(ServiceConfiguration serviceConfiguration, string outputFormat = "YAML")
        {
            switch (outputFormat.ToUpper())
            {
                case "JSON":
                    {
                        return WriteToJson(serviceConfiguration);
                    }
                case "CS":
                    {
                        return "This format is not implemented yet.";
                    }
                case "YAML":
                default:
                    {
                        return WriteToYaml(serviceConfiguration);
                    }
            }
        }

        public string WriteToJson(ServiceConfiguration serviceConfiguration)
        {
            return SerializeToJson(serviceConfiguration);
        }

        public string WriteToYaml(ServiceConfiguration serviceConfiguration)
        {
            return SerializeToYaml(serviceConfiguration);
        }

        public ServiceConfiguration GetServiceConfiguration()
        {
            ServiceConfiguration serviceConfiguration = new ServiceConfiguration
            {
                //TODO: make the getters accept nullable types (e.g. 'bool?')
                TrackServiceApiCalls = _serviceConfiguration.Get<bool>(ConfigurationCollection.Parameters.TrackServiceApiCalls),
                LogServiceApiCalls = _serviceConfiguration.Get<bool>(ConfigurationCollection.Parameters.LogServiceApiCalls),
                DefaultHttpStatusCode = _serviceConfiguration.Get<int>(ConfigurationCollection.Parameters.DefaultHttpStatusCode),
                DefaultContentType = _serviceConfiguration.Get<string>(ConfigurationCollection.Parameters.DefaultContentType),

                EndpointDescriptions = _routeMatcher.GetAllRoutes().ToArray()
            };

            return serviceConfiguration;
        }

        private string SerializeToJson<TObject>(TObject value)
        {
            string result = JsonConvert.SerializeObject(value, Formatting.Indented);
            return result;
        }

        private string SerializeToYaml<TObject>(TObject value)
        {
            StringWriter stringWriter = new StringWriter();
            Serializer serializer = new Serializer();
            serializer.Serialize(stringWriter, value);
            return stringWriter.ToString();
        }

    }
}
