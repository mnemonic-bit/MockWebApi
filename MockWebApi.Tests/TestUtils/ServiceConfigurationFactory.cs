using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Service.Rest;
using System;

namespace MockWebApi.Tests.TestUtils
{
    internal static class ServiceConfigurationFactory
    {

        public static void AddEndpointDescription(this IServiceConfiguration serviceConfiguration, EndpointDescription endpointDescription)
        {
            serviceConfiguration.RouteMatcher.AddRoute(endpointDescription.Route, endpointDescription);
        }

        public static IServiceConfiguration CreateBaseConfiguration(string serviceName)
        {
            IServiceConfiguration serviceConfiguration = new ServiceConfiguration()
            {
                ServiceName = serviceName,
                Url = MockHostBuilder.DEFAULT_MOCK_BASE_URL,
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

        public static MockedServiceConfiguration CreateMockedServiceConfiguration()
        {
            MockedServiceConfiguration config = new MockedServiceConfiguration();

            config.ServiceName = "TEST-SERVICE";
            config.BaseUrl = MockHostBuilder.DEFAULT_MOCK_BASE_URL;

            DefaultEndpointDescription defaultEndpointDescription = new DefaultEndpointDescription()
            {
                CheckAuthorization = false,
                Result = new HttpResult()
                {
                    Body = "",
                    ContentType = "text/plain",
                    StatusCode = System.Net.HttpStatusCode.OK
                },
                ReturnCookies = false
            };

            config.DefaultEndpointDescription = defaultEndpointDescription;

            return config;
        }

    }
}
