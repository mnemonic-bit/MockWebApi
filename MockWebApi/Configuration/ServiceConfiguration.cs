﻿using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Routing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace MockWebApi.Configuration
{
    /// <summary>
    /// This class describes completely the configuration of a single service.
    /// The mock server can host multiple services, each of which will be described
    /// by one of these instances.
    /// </summary>
    public class ServiceConfiguration : IServiceConfiguration
    {

        public string ServiceName { get; private set; }

        public string Url { get; private set; }

        public DefaultEndpointDescription DefaultEndpointDescription { get; set; }

        public DefaultEndpointDescription ErrorResponseEndpointDescription { get; set; }

        public JwtServiceOptions JwtServiceOptions { get; set; }

        public IConfigurationCollection ConfigurationCollection { get; private set; }

        public IRouteMatcher<IEndpointState> RouteMatcher { get; private set; }

        public ServiceConfiguration(string serviceName, string url)
        {
            ServiceName = serviceName;
            Url = url;
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
                SigningKey = "This is the default key set by the mock web api on startup and whenever you reset the service to default settings"
            };

            return jwtServiceOptions;
        }

    }
}
