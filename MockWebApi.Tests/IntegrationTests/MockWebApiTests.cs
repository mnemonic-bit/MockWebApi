using MockWebApi.Configuration.Model;
using MockWebApi.Tests.TestUtils;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

using MockWebApiClient = MockWebApi.Client.MockWebApi;

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

            EndpointDescription endpointConfiguration = new EndpointDescription()
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
            EndpointDescription[] endpointConfigurations = await webApiClient.GetRoutes();

            // Assert
            Assert.True(configureWebApiResult);
            Assert.True(configureRouteResult);
            Assert.Single(endpointConfigurations);
        }

        [Fact]
        public async Task ConfigureRoute_ShouldReturnBody_WhenRouteIsConfigured()
        {
            // Arrange
            IntegrationTestServer integrationTestServer = new IntegrationTestServer();
            HttpClient httpClient = integrationTestServer.CreateHttpClient();
            HttpTestClient httpTestClient = new HttpTestClient(httpClient);

            string testUriPath = "/brand/new/path";
            string expectedResponseBody = "some: body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            EndpointDescription endpointConfiguration = new EndpointDescription()
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

            MockWebApiClient webApiClient = new MockWebApiClient(httpClient);
            bool configureRouteResult = await webApiClient.ConfigureRoute(endpointConfiguration);

            // Act
            HttpResponseMessage responseMessage = await httpTestClient.SendMessage(testUriPath, "Test message");

            // Assert
            Assert.True(configureRouteResult);
            Assert.Equal(statusCode, responseMessage.StatusCode);

            string responseBody = await responseMessage.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponseBody, responseBody);
        }

        [Fact]
        public async Task ConfigureRoute_ShouldReturnDefaultResponse_WhenRouteIsDeleted()
        {
            // Arrange
            IntegrationTestServer integrationTestServer = new IntegrationTestServer();
            HttpClient httpClient = integrationTestServer.CreateHttpClient();
            HttpTestClient httpTestClient = new HttpTestClient(httpClient);

            string testUriPath = "/brand/new/path";
            string expectedResponseBody = "some: body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            EndpointDescription endpointConfiguration = new EndpointDescription()
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

            MockWebApiClient webApiClient = new MockWebApiClient(httpClient);
            bool configureRouteResult = await webApiClient.ConfigureRoute(endpointConfiguration);
            bool deleteRouteResult = await webApiClient.DeleteRoute(endpointConfiguration.Route);

            // Act
            HttpResponseMessage responseMessage = await httpTestClient.SendMessage(testUriPath, "Test message");

            // Assert
            Assert.True(configureRouteResult);
            Assert.True(deleteRouteResult);
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

            string responseBody = await responseMessage.Content.ReadAsStringAsync();
            Assert.Empty(responseBody);
        }

    }
}
