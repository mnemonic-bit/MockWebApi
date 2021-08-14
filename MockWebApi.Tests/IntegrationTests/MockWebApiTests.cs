using MockWebApiClient = MockWebApi.Client.MockWebApi;
using System;
using Xunit;
using MockWebApi.Client.Model;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using MockWebApi.Tests.TestUtils;

namespace MockWebApi.IntegrationTests.Tests
{
    public class MockWebApiTests
    {

        [Fact]
        public async Task ConfigureRoute_ShouldConfigureRoute()
        {
            // Arrange
            string testUriPath = "/brand/new/path";
            string responseBody = "some body";

            EndpointConfiguration endpointConfiguration = new EndpointConfiguration()
            {
                Route = testUriPath,
                LifecyclePolicy = LifecyclePolicy.ApplyOnce,
                RequestBodyType = "text/plain",
                Results = new HttpResult[]
                {
                    new HttpResult()
                    {
                        ContentType = "application/yaml",
                        StatusCode = HttpStatusCode.Created,
                        Body = responseBody
                    }
                }
            };

            MockWebApiClient webApiClient = new MockWebApiClient(new Uri("http://localhost:5000"));
            bool configureWebApiResult = await webApiClient.Configure(trackServiceApiCalls: true);
            bool configureRouteResult = await webApiClient.ConfigureRoute(endpointConfiguration);

            // Act
            EndpointConfiguration[] endpointConfigurations = await webApiClient.GetRoutes();

            // Assert
            Assert.True(configureWebApiResult);
            Assert.True(configureRouteResult);
            Assert.Single(endpointConfigurations);
        }

        [Fact]
        public async Task ConfigureRoute_ShouldReturnBody_WhenRouteIsConfigured()
        {
            // Arrange
            Uri serverUri = new Uri("http://localhost:5000");
            string testUriPath = "/brand/new/path";
            string expectedResponseBody = "some: body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            EndpointConfiguration endpointConfiguration = new EndpointConfiguration()
            {
                Route = testUriPath,
                LifecyclePolicy = LifecyclePolicy.ApplyOnce,
                RequestBodyType = "text/plain",
                Results = new HttpResult[]
                {
                    new HttpResult()
                    {
                        ContentType = "application/yaml",
                        StatusCode = statusCode,
                        Body = expectedResponseBody
                    }
                }
            };

            MockWebApiClient webApiClient = new MockWebApiClient(serverUri);
            bool configureWebApiResult = await webApiClient.Configure(trackServiceApiCalls: true);
            bool configureRouteResult = await webApiClient.ConfigureRoute(endpointConfiguration);

            // Act
            HttpResponseMessage responseMessage = await HttpTestClient.SendMessage(serverUri, testUriPath, "Test message");

            // Assert
            Assert.True(configureWebApiResult);
            Assert.True(configureRouteResult);
            Assert.Equal(statusCode, responseMessage.StatusCode);

            string responseBody = await responseMessage.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponseBody, responseBody);
        }

    }
}
