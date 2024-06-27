using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Routing;

namespace MockWebApi.Configuration
{
    public class ServiceConfigurationProxy : IRestServiceConfiguration
    {

        public IRestServiceConfiguration BaseConfiguration { get; set; }

        public string ServiceName { get => BaseConfiguration.ServiceName; }

        public string Url { get => BaseConfiguration.Url; }

        public bool TrackServiceApiCalls { get => BaseConfiguration.TrackServiceApiCalls; }

        public string ServiceType { get => BaseConfiguration.ServiceType; }

        public DefaultEndpointDescription DefaultEndpointDescription { get => BaseConfiguration.DefaultEndpointDescription; set => BaseConfiguration.DefaultEndpointDescription = value; }

        public DefaultEndpointDescription ErrorResponseEndpointDescription { get => BaseConfiguration.ErrorResponseEndpointDescription; set => BaseConfiguration.ErrorResponseEndpointDescription = value; }

        public JwtServiceOptions JwtServiceOptions { get => BaseConfiguration.JwtServiceOptions; set => BaseConfiguration.JwtServiceOptions = value; }

        public IConfigurationCollection ConfigurationCollection => BaseConfiguration.ConfigurationCollection;

        public IRouteMatcher<IEndpointState> RouteMatcher => BaseConfiguration.RouteMatcher;

        public ServiceConfigurationProxy(IRestServiceConfiguration configuration)
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
