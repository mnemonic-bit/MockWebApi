using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Routing;
using System;
using System.Net;

namespace MockWebApi.Configuration
{
    public class ServiceConfiguration : IServiceConfiguration
    {

        public DefaultEndpointDescription DefaultEndpointDescription { get; set; }

        public JwtServiceOptions JwtServiceOptions { get; set; }

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
            JwtServiceOptions = serviceConfiguration.JwtServiceOptions;
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
            JwtServiceOptions = CreateJwtServiceOptions();
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

        private JwtServiceOptions CreateJwtServiceOptions()
        {
            JwtServiceOptions jwtServiceOptions = new JwtServiceOptions()
            {
                Audience = "AUDIENCE",
                Issuer = "ISSUER",
                Expiration = TimeSpan.FromHours(1),
                SigningKey = "This is the default key set by the mock web api on startup and whenever you reset the service to default settings"
            };

            return jwtServiceOptions;
        }

    }
}
