using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Routing;

namespace MockWebApi.Configuration
{
    /// <summary>
    /// This class describes completely the configuration of a single service.
    /// The mock server can host multiple services, each of which will be described
    /// by one of these instances.
    /// </summary>
    public class RestServiceConfiguration : IRestServiceConfiguration
    {

        public string ServiceName { get; private set; }

        public string Url { get; private set; }

        public bool TrackServiceApiCalls { get; private set; }

        public string ServiceType { get; private set; }

        public DefaultEndpointDescription DefaultEndpointDescription { get; set; }

        public DefaultEndpointDescription ErrorResponseEndpointDescription { get; set; }

        public JwtServiceOptions JwtServiceOptions { get; set; }

        public IConfigurationCollection ConfigurationCollection { get; private set; }

        public IRouteMatcher<IEndpointState> RouteMatcher { get; private set; }

        /// <summary>
        /// Initializes a new service configuration for a service with a given
        /// name, and a specific URL. If no service type is provided, the service
        /// will provide a REST interface.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="url"></param>
        /// <param name="serviceType"></param>
        public RestServiceConfiguration(string serviceName, string url)
        {
            ServiceName = serviceName;
            Url = url;
            ResetToDefault();
            ServiceType = "REST";
        }

        public bool ReadFromYaml(string configYaml)
        {
            RestServiceConfiguration deserializedServiceConfiguration = configYaml.DeserializeYaml<RestServiceConfiguration>();
            InitFrom(deserializedServiceConfiguration);

            return true;
        }

        [MemberNotNull(nameof(ConfigurationCollection))]
        [MemberNotNull(nameof(RouteMatcher))]
        [MemberNotNull(nameof(DefaultEndpointDescription))]
        [MemberNotNull(nameof(ErrorResponseEndpointDescription))]
        [MemberNotNull(nameof(JwtServiceOptions))]
        public void ResetToDefault()
        {
            ConfigurationCollection = new ConfigurationCollection();
            RouteMatcher = new RouteGraphMatcher<IEndpointState>(new RouteParser());
            DefaultEndpointDescription = CreateDefaultEndpointDescription();
            ErrorResponseEndpointDescription = CreateErrorResponseEndpointDescription();
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
                    StatusCode = HttpStatusCode.NotFound,
                    ContentType = "text/plain"
                }
            };

            return defaultEndpointDescription;
        }

        private DefaultEndpointDescription CreateErrorResponseEndpointDescription()
        {
            DefaultEndpointDescription errorResponseEndpointDescription = new DefaultEndpointDescription()
            {
                CheckAuthorization = false,
                AllowedUsers = new string[] { },
                ReturnCookies = true,
                Result = new HttpResult()
                {
                    StatusCode = HttpStatusCode.BadRequest
                }
            };

            return errorResponseEndpointDescription;
        }

        private JwtServiceOptions CreateJwtServiceOptions()
        {
            JwtServiceOptions jwtServiceOptions = new JwtServiceOptions()
            {
                Audience = "AUDIENCE",
                Issuer = "ISSUER",
                Expiration = TimeSpan.FromHours(1),
                SigningKey = "This is the default key set by the MockWebApi on startup and whenever you reset the service to the default settings"
            };

            return jwtServiceOptions;
        }

        private void InitFrom(RestServiceConfiguration serviceConfiguration)
        {
            DefaultEndpointDescription = serviceConfiguration.DefaultEndpointDescription;
            ConfigurationCollection = serviceConfiguration.ConfigurationCollection;
            RouteMatcher = serviceConfiguration.RouteMatcher;
            JwtServiceOptions = serviceConfiguration.JwtServiceOptions;
        }

    }
}
