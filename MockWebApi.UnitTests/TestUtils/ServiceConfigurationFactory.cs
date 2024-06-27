using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Service.Rest;
using System;

namespace MockWebApi.Tests.TestUtils
{
    internal static class ServiceConfigurationFactory
    {

        public static void AddEndpointDescription(this IRestServiceConfiguration serviceConfiguration, EndpointDescription endpointDescription)
        {
            serviceConfiguration.RouteMatcher.AddRoute(endpointDescription.Route, new EndpointState(endpointDescription));
        }

        public static IRestServiceConfiguration CreateBaseConfiguration(string serviceName)
        {
            IRestServiceConfiguration serviceConfiguration = new RestServiceConfiguration(serviceName, DefaultValues.DEFAULT_MOCK_BASE_URL)
            {
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

        public static MockedRestServiceConfiguration CreateMockedServiceConfiguration()
        {
            MockedRestServiceConfiguration config = new MockedRestServiceConfiguration();

            config.ServiceName = "TEST-SERVICE";
            config.BaseUrl = DefaultValues.DEFAULT_MOCK_BASE_URL;

            DefaultEndpointDescription defaultEndpointDescription = new DefaultEndpointDescription()
            {
                CheckAuthorization = false,
                Result = new HttpResult()
                {
                    Body = string.Empty,
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
