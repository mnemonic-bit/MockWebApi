using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Tests.TestUtils;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

using MockWebApiClient = MockWebApi.Client.MockWebApi;

namespace MockWebApi.Tests.IntegrationTests
{
    public class ServiceApiTests
    {

        [Fact]
        public async Task ConfigureRoute_ShouldReturnBody_WhenRouteIsConfigured()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";

            IServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);

            MockWebApiTestServer integrationTestServer = new MockWebApiTestServer(serviceConfiguration);
            HttpClient httpClient = integrationTestServer.CreateHttpClient();
            HttpTestClient httpTestClient = new HttpTestClient(httpClient);

            string testUriPath = "/brand/new/path";
            string expectedResponseBody = "some: body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            EndpointDescription endpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                testUriPath,
                statusCode,
                expectedResponseBody);

            MockWebApiClient webApiClient = new MockWebApiClient(httpClient);
            bool configureRouteResult = await webApiClient.ConfigureRoute(serviceName, endpointConfiguration);

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
            string serviceName = "TEST-SERVICE";

            IServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);

            MockWebApiTestServer integrationTestServer = new MockWebApiTestServer(serviceConfiguration);
            HttpClient httpClient = integrationTestServer.CreateHttpClient();
            HttpTestClient httpTestClient = new HttpTestClient(httpClient);

            string testUriPath = "/brand/new/path";
            string expectedResponseBody = "some: body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            EndpointDescription endpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                testUriPath,
                statusCode,
                expectedResponseBody);

            MockWebApiClient webApiClient = new MockWebApiClient(httpClient);
            bool configureRouteResult = await webApiClient.ConfigureRoute(serviceName, endpointConfiguration);
            bool deleteRouteResult = await webApiClient.DeleteRoute(serviceName, endpointConfiguration.Route);

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
