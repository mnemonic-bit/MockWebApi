using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Tests.TestUtils;
using System;
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
        public async Task HttpRequest_ShouldReturnSuccessfully_WhenRouteHasBeenConfigured()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";

            IServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);

            ServiceApiTestServer serviceApiTestServer = new ServiceApiTestServer(serviceConfiguration);

            HttpClient httpClient = serviceApiTestServer.CreateHttpClient();
            MockWebApiClient webApiClient = new MockWebApiClient(httpClient);
            HttpTestClient httpTestClient = new HttpTestClient();

            string testUriPath = "/brand/new/path";
            string expectedResponseBody = "some: body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            EndpointDescription endpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                testUriPath,
                statusCode,
                expectedResponseBody);

            bool mockeApiHasStarted = await webApiClient.StartNewMockWebApi(serviceName);

            // Act
            bool configureRouteResult = await webApiClient.ConfigureRoute(serviceName, endpointConfiguration);
            HttpResponseMessage response = await httpTestClient.SendMessage(new Uri("http://localhost:5000"), testUriPath);

            // Assert
            Assert.True(mockeApiHasStarted);
            Assert.True(configureRouteResult);
            Assert.True(response.IsSuccessStatusCode);
        }

    }
}
