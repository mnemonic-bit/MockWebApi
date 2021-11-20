using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Routing;

namespace MockWebApi.Configuration
{
    public class ServiceConfigurationProxy : IServiceConfiguration
    {

        public IServiceConfiguration BaseConfiguration { get; set; }

        public string ServiceName { get => BaseConfiguration.ServiceName; set => BaseConfiguration.ServiceName = value; }

        public string Url { get => BaseConfiguration.Url; set => BaseConfiguration.Url = value; }

        public DefaultEndpointDescription DefaultEndpointDescription { get => BaseConfiguration.DefaultEndpointDescription; set => BaseConfiguration.DefaultEndpointDescription = value; }

        public DefaultEndpointDescription ErrorResponseEndpointDescription { get => BaseConfiguration.ErrorResponseEndpointDescription; set => BaseConfiguration.ErrorResponseEndpointDescription = value; }

        public JwtServiceOptions JwtServiceOptions { get => BaseConfiguration.JwtServiceOptions; set => BaseConfiguration.JwtServiceOptions = value; }

        public IConfigurationCollection ConfigurationCollection => BaseConfiguration.ConfigurationCollection;

        public IRouteMatcher<EndpointDescription> RouteMatcher => BaseConfiguration.RouteMatcher;

        public ServiceConfigurationProxy(IServiceConfiguration configuration)
        {
            BaseConfiguration = configuration;
        }

        public bool ReadFromYaml(string configYaml)
        {
            return BaseConfiguration.ReadFromYaml(configYaml);
        }

        public void ResetToDefault()
        {
            BaseConfiguration.ResetToDefault();
        }
    }
}
