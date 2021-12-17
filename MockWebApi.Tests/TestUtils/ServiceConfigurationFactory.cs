using System;

using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;

namespace MockWebApi.Tests.TestUtils
{
    internal static class ServiceConfigurationFactory
    {

        public static IServiceConfiguration CreateBaseConfiguration()
        {
            return CreateBaseConfiguration("TEST-SERVICE");
        }

        public static IServiceConfiguration CreateBaseConfiguration(string serviceName)
        {
            IServiceConfiguration serviceConfiguration = new ServiceConfiguration()
            {
                ServiceName = serviceName,
                Url = "http://localhost:5000",
                JwtServiceOptions = new JwtServiceOptions()
                {
                    Issuer = "INTEGRATION-TEST",
                    Audience = "INTEGRATION-TESTERS",
                    Expiration = TimeSpan.FromSeconds(20),
                    SigningKey = "SUPER-SECRET-SIGNING-KEY-FOR-INTEGRATION-TESTS"
                }
            };

            return serviceConfiguration;
        }

        public static void AddEndpointDescription(this IServiceConfiguration serviceConfiguration, EndpointDescription endpointDescription)
        {
            serviceConfiguration.RouteMatcher.AddRoute(endpointDescription.Route, endpointDescription);
        }

    }
}
