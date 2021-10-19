using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Routing;
using System.Net;

namespace MockWebApi.Configuration
{
    public class ServiceConfiguration : IServiceConfiguration
    {

        public DefaultEndpointDescription DefaultEndpointDescription { get; set; }

        public IConfigurationCollection ConfigurationCollection { get; private set; }

        public IRouteMatcher<EndpointDescription> RouteMatcher { get; private set; }

        public ServiceConfiguration()
        {
            ResetToDefault();
        }

        public void InitFrom(ServiceConfiguration serviceConfiguration)
        {
            DefaultEndpointDescription = serviceConfiguration.DefaultEndpointDescription;
            ConfigurationCollection = serviceConfiguration.ConfigurationCollection;
            RouteMatcher = serviceConfiguration.RouteMatcher;
        }

        public bool ReadFromYaml(string configYaml)
        {
            ServiceConfiguration deserializedServiceConfiguration = configYaml.DeserializeYaml<ServiceConfiguration>();
            InitFrom(deserializedServiceConfiguration);

            return true;
        }

        public void ResetToDefault()
        {
            ConfigurationCollection = new ConfigurationCollection();
            RouteMatcher = new RouteGraphMatcher<EndpointDescription>();
            DefaultEndpointDescription = CreateDefaultEndpointDescription();
        }

        private DefaultEndpointDescription CreateDefaultEndpointDescription()
        {
            DefaultEndpointDescription defaultEndpointDescription = new DefaultEndpointDescription()
            {
                CheckAuthorization = false,
                AllowedUsers = new string[] { },
                ReturnCookies = true,
                Result = new HttpResult()
                {
                    StatusCode = HttpStatusCode.OK
                }
            };

            return defaultEndpointDescription;
        }

    }
}
