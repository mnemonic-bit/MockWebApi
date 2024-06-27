using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.FunctionalTests.TestUtils;
using MockWebApi.Test;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

using MockWebApiClient = MockWebApi.Client.MockWebApi;

namespace MockWebApi.FunctionalTests
{
    [Collection("SequentialIntegrationTests")]
    public class ServiceApiTests
    {

        [Fact]
        public async Task HttpRequest_ShouldReturnSuccessfully_WhenRouteHasBeenConfigured()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            string baseUrl = "http://localhost:5000";

            IRestServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);

            using ServiceApiTestServer serviceApiTestServer = new ServiceApiTestServer(serviceConfiguration);

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

            MockedRestServiceConfiguration config = ServiceConfigurationFactory.CreateMockedServiceConfiguration();

            bool mockeApiHasStarted = await webApiClient.StartNewMockWebApi(serviceName, config);

            // Act
            bool configureRouteResult = await webApiClient.ConfigureRoute(serviceName, endpointConfiguration);
            HttpResponseMessage response = await httpTestClient.SendMessage(new Uri(baseUrl), testUriPath);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(mockeApiHasStarted);
            Assert.True(configureRouteResult);
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(statusCode, response.StatusCode);
            Assert.Equal(expectedResponseBody, responseContent);
        }

        [Fact]
        public async Task StopNewMockWebApi_ShouldRemovePortBinding()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            string testUriPath = "/brand/new/path";
            HttpStatusCode defaultStatusCode = HttpStatusCode.NotFound;
            string baseUrl = "http://localhost:5000";

            IRestServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);
            serviceConfiguration.DefaultEndpointDescription.Result.StatusCode = defaultStatusCode;

            using ServiceApiTestServer serviceApiTestServer = new ServiceApiTestServer(serviceConfiguration);

            HttpClient httpClient = serviceApiTestServer.CreateHttpClient();
            MockWebApiClient webApiClient = new MockWebApiClient(httpClient);
            HttpTestClient httpTestClient = new HttpTestClient();

            MockedRestServiceConfiguration config = ServiceConfigurationFactory.CreateMockedServiceConfiguration();
            config.DefaultEndpointDescription.Result.StatusCode = defaultStatusCode;

            // Act
            bool mockeApiHasStarted = await webApiClient.StartNewMockWebApi(serviceName, config);

            HttpResponseMessage response = await httpTestClient.SendMessage(new Uri(baseUrl), testUriPath);
            string responseContent = await response.Content.ReadAsStringAsync();

            bool mockeApiHasStopped = await webApiClient.StopMockWebApi(serviceName);
            bool mockeApiHasStartedAgain = await webApiClient.StartNewMockWebApi(serviceName, config);

            // Assert
            Assert.True(mockeApiHasStarted);
            Assert.True(mockeApiHasStopped);
            Assert.False(response.IsSuccessStatusCode);
            Assert.True(string.IsNullOrEmpty(responseContent));
            Assert.True(mockeApiHasStartedAgain);
        }

        [Fact]
        public async Task HttpRequest_ShouldReturnDefaultResponse_WhenRouteHasLifecycleOfApplyOnce()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            HttpStatusCode defaultStatusCode = HttpStatusCode.NotFound;
            string baseUrl = "http://localhost:5000";

            IRestServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);
            serviceConfiguration.DefaultEndpointDescription.Result.StatusCode = defaultStatusCode;

            using ServiceApiTestServer serviceApiTestServer = new ServiceApiTestServer(serviceConfiguration);

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

            MockedRestServiceConfiguration config = ServiceConfigurationFactory.CreateMockedServiceConfiguration();
            config.DefaultEndpointDescription.Result.StatusCode = defaultStatusCode;

            bool mockeApiHasStarted = await webApiClient.StartNewMockWebApi(serviceName, config);

            // Act
            bool configureRouteResult = await webApiClient.ConfigureRoute(serviceName, endpointConfiguration);
            HttpResponseMessage response1 = await httpTestClient.SendMessage(new Uri(baseUrl), testUriPath);
            HttpResponseMessage response2 = await httpTestClient.SendMessage(new Uri(baseUrl), testUriPath);

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
            HttpStatusCode defaultStatusCode = HttpStatusCode.NotFound;
            string baseUrl = "http://localhost:5000";

            IRestServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);

            using ServiceApiTestServer serviceApiTestServer = new ServiceApiTestServer(serviceConfiguration);

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

            endpointConfiguration.LifecyclePolicy = LifecyclePolicy.Repeat;

            MockedRestServiceConfiguration config = ServiceConfigurationFactory.CreateMockedServiceConfiguration();
            config.DefaultEndpointDescription.Result.StatusCode = defaultStatusCode;

            bool mockeApiHasStarted = await webApiClient.StartNewMockWebApi(serviceName, config);

            // Act
            bool configureRouteResult = await webApiClient.ConfigureRoute(serviceName, endpointConfiguration);
            HttpResponseMessage configuredResponse = await httpTestClient.SendMessage(new Uri(baseUrl), testUriPath);
            bool deleteRouteResult = await webApiClient.DeleteRoute(serviceName, testUriPath);
            HttpResponseMessage deleteResponse = await httpTestClient.SendMessage(new Uri(baseUrl), testUriPath);

            // Assert
            Assert.True(mockeApiHasStarted);
            Assert.True(configureRouteResult);
            Assert.True(deleteRouteResult);
            Assert.True(configuredResponse.IsSuccessStatusCode);
            Assert.Equal(statusCode, configuredResponse.StatusCode);
            Assert.Equal(defaultStatusCode, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task ConfigureService_ShouldStartAndSetUpTheService_WhenServiceDoesNotExist()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            HttpStatusCode defaultStatusCode = HttpStatusCode.NotFound;
            string baseUrl = "http://localhost:5000";

            string testUriPath = "/brand/new/path";
            string expectedResponseBody = "some: body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            IRestServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);

            using ServiceApiTestServer serviceApiTestServer = new ServiceApiTestServer(serviceConfiguration);

            HttpClient httpClient = serviceApiTestServer.CreateHttpClient();
            MockWebApiClient webApiClient = new MockWebApiClient(httpClient);
            HttpTestClient httpTestClient = new HttpTestClient();

            EndpointDescription endpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                testUriPath,
                statusCode,
                expectedResponseBody);

            endpointConfiguration.LifecyclePolicy = LifecyclePolicy.Repeat;

            MockedRestServiceConfiguration config = ServiceConfigurationFactory.CreateMockedServiceConfiguration();

            config.ServiceName = serviceName;
            config.BaseUrl = baseUrl;
            config.DefaultEndpointDescription.Result.StatusCode = defaultStatusCode;
            config.EndpointDescriptions = new EndpointDescription[] { endpointConfiguration };

            // Act
            bool mockedApiWasConfigured = await webApiClient.ConfigureMockWebApi(config);
            HttpResponseMessage response = await httpTestClient.SendMessage(new Uri(baseUrl), testUriPath);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(mockedApiWasConfigured);
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(statusCode, response.StatusCode);
            Assert.Equal(expectedResponseBody, responseContent);
        }

        [Fact]
        public async Task ConfigureService_ShouldSetUpTheService_WhenServiceAlreadyExist()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            HttpStatusCode defaultStatusCode = HttpStatusCode.NotFound;
            string baseUrl = "http://localhost:5000";

            string testUriPath = "/brand/new/path";
            string expectedResponseBody = "some: body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            IRestServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);

            using ServiceApiTestServer serviceApiTestServer = new ServiceApiTestServer(serviceConfiguration);

            HttpClient httpClient = serviceApiTestServer.CreateHttpClient();
            MockWebApiClient webApiClient = new MockWebApiClient(httpClient);
            HttpTestClient httpTestClient = new HttpTestClient();

            EndpointDescription endpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                testUriPath,
                statusCode,
                expectedResponseBody);

            endpointConfiguration.LifecyclePolicy = LifecyclePolicy.Repeat;

            MockedRestServiceConfiguration config = ServiceConfigurationFactory.CreateMockedServiceConfiguration();

            config.ServiceName = serviceName;
            config.BaseUrl = baseUrl;
            config.DefaultEndpointDescription.Result.StatusCode = defaultStatusCode;

            bool mockeApiHasStarted = await webApiClient.StartNewMockWebApi(serviceName);

            // Act
            bool mockedApiWasConfigured = await webApiClient.ConfigureMockWebApi(config);
            bool configureRouteResult = await webApiClient.ConfigureRoute(serviceName, endpointConfiguration);
            HttpResponseMessage response = await httpTestClient.SendMessage(new Uri(baseUrl), testUriPath);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(mockeApiHasStarted);
            Assert.True(configureRouteResult);
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(statusCode, response.StatusCode);
            Assert.Equal(expectedResponseBody, responseContent);
        }

        [Fact]
        public async Task ConfigureService_ShouldDropRoutes_WhenNewConfigDoesNotContainThem()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            HttpStatusCode defaultStatusCode = HttpStatusCode.NotFound;
            string baseUrl = "http://localhost:5000";

            string oldUriPath = "/this/path/will/be/dropped/after/configuration";
            string testUriPath = "/brand/new/path";
            string expectedResponseBody = "some: body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            IRestServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);

            using ServiceApiTestServer serviceApiTestServer = new ServiceApiTestServer(serviceConfiguration);

            HttpClient httpClient = serviceApiTestServer.CreateHttpClient();
            MockWebApiClient webApiClient = new MockWebApiClient(httpClient);
            HttpTestClient httpTestClient = new HttpTestClient();

            EndpointDescription oldEndpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                oldUriPath,
                statusCode,
                expectedResponseBody);

            oldEndpointConfiguration.LifecyclePolicy = LifecyclePolicy.Repeat;

            EndpointDescription newEndpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                testUriPath,
                statusCode,
                expectedResponseBody);

            newEndpointConfiguration.LifecyclePolicy = LifecyclePolicy.Repeat;

            MockedRestServiceConfiguration config = ServiceConfigurationFactory.CreateMockedServiceConfiguration();

            config.ServiceName = serviceName;
            config.BaseUrl = baseUrl;
            config.DefaultEndpointDescription.Result.StatusCode = defaultStatusCode;
            config.EndpointDescriptions = new EndpointDescription[] { newEndpointConfiguration };

            bool mockedApiHasStarted = await webApiClient.StartNewMockWebApi(serviceName);

            // Act
            bool configureRouteResult = await webApiClient.ConfigureRoute(serviceName, oldEndpointConfiguration);
            bool mockedApiWasConfigured = await webApiClient.ConfigureMockWebApi(config);
            HttpResponseMessage oldResponse = await httpTestClient.SendMessage(new Uri(baseUrl), oldUriPath);
            string oldResponseContent = await oldResponse.Content.ReadAsStringAsync();
            HttpResponseMessage response = await httpTestClient.SendMessage(new Uri(baseUrl), testUriPath);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(mockedApiHasStarted);
            Assert.True(mockedApiWasConfigured);
            Assert.True(configureRouteResult);
            Assert.True(string.IsNullOrEmpty(oldResponseContent));
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(statusCode, response.StatusCode);
            Assert.Equal(expectedResponseBody, responseContent);
        }

    }
}
