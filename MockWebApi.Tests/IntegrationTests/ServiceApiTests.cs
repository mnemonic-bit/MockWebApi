using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Tests.TestUtils;

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
            Assert.Equal(statusCode, response.StatusCode);
        }

        [Fact]
        public async Task HttpRequest_ShouldReturnDefaultResponse_WhenRouteHasLifecycleOfApplyOnce()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            HttpStatusCode defaultStatusCode = HttpStatusCode.NotFound;

            IServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);
            serviceConfiguration.DefaultEndpointDescription.Result.StatusCode = defaultStatusCode;

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

            endpointConfiguration.LifecyclePolicy = LifecyclePolicy.ApplyOnce;

            bool mockeApiHasStarted = await webApiClient.StartNewMockWebApi(serviceName);

            // Act
            bool configureRouteResult = await webApiClient.ConfigureRoute(serviceName, endpointConfiguration);
            HttpResponseMessage response1 = await httpTestClient.SendMessage(new Uri("http://localhost:5000"), testUriPath);
            HttpResponseMessage response2 = await httpTestClient.SendMessage(new Uri("http://localhost:5000"), testUriPath);

            // Assert
            Assert.True(mockeApiHasStarted);
            Assert.True(configureRouteResult);
            Assert.Equal(statusCode, response1.StatusCode);
            Assert.Equal(defaultStatusCode, response2.StatusCode);
        }

        [Fact]
        public async Task HttpRequest_ShouldReturnDefaultResponse_WhenRouteHasBeenConfiguredAndThenDeleted()
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
            HttpResponseMessage configuredResponse = await httpTestClient.SendMessage(new Uri("http://localhost:5000"), testUriPath);
            bool deleteRouteResult = await webApiClient.DeleteRoute(serviceName, testUriPath);
            HttpResponseMessage deleteResponse = await httpTestClient.SendMessage(new Uri("http://localhost:5000"), testUriPath);

            // Assert
            Assert.True(mockeApiHasStarted);
            Assert.True(configureRouteResult);
            Assert.True(deleteRouteResult);
            Assert.True(configuredResponse.IsSuccessStatusCode);
            Assert.Equal(statusCode, configuredResponse.StatusCode);
            Assert.True(deleteResponse.IsSuccessStatusCode);
            Assert.Equal(statusCode, deleteResponse.StatusCode);
        }

    }
}
