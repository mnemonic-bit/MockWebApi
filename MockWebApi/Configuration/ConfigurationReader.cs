using MockWebApi.Data;
using MockWebApi.Model;
using MockWebApi.Routing;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace MockWebApi.Configuration
{
    public class ConfigurationReader : IConfigurationReader
    {

        private readonly IConfigurationCollection _serviceConfiguration;
        private readonly IRouteMatcher<EndpointDescription> _routeMatcher;

        public ConfigurationReader(
            IConfigurationCollection serviceConfiguration,
            IRouteMatcher<EndpointDescription> routeMatcher)
        {
            _serviceConfiguration = serviceConfiguration;
            _routeMatcher = routeMatcher;
        }

        public ServiceConfiguration ReadFromJson(string text)
        {
            ServiceConfiguration configuration = DeserializeJson<ServiceConfiguration>(text);
            return configuration;
        }

        public ServiceConfiguration ReadFromYaml(string text)
        {
            ServiceConfiguration config = DeserializeYaml<ServiceConfiguration>(text);
            return config;
        }

        public void ConfigureService(ServiceConfiguration configuration)
        {
            _serviceConfiguration.Set(ConfigurationCollection.Parameters.TrackServiceApiCalls, configuration?.TrackServiceApiCalls ?? false);
            _serviceConfiguration.Set(ConfigurationCollection.Parameters.LogServiceApiCalls, configuration?.LogServiceApiCalls ?? false);
            _serviceConfiguration.Set(ConfigurationCollection.Parameters.DefaultHttpStatusCode, configuration?.DefaultHttpStatusCode ?? 200);
            _serviceConfiguration.Set(ConfigurationCollection.Parameters.DefaultContentType, configuration.DefaultContentType ?? "text/plain");

            if (configuration?.EndpointDescriptions == null)
            {
                return;
            }

            foreach (EndpointDescription endpoint in configuration.EndpointDescriptions)
            {
                _routeMatcher.AddRoute(endpoint.Route, endpoint);
            }
        }

        private T DeserializeYaml<T>(string yamlText)
        {
            IDeserializer deserializer = new DeserializerBuilder()
                //.WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<T>(yamlText);
        }

        private T DeserializeJson<T>(string jsonText)
        {
            T result = JsonConvert.DeserializeObject<T>(jsonText);
            return result;
        }

    }
}
