using MockWebApi.Auth;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Test;
using MockWebApi.Tests.TestUtils;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace MockWebApi.Tests.IntegrationTests
{
    /// <summary>
    /// This tests use a instant-mock-server which is build up during
    /// each test.
    /// </summary>
    public class MockWebApiTests
    {

        [Fact]
        public async Task MockWebApi_ShouldReturnConfguredResponse_WhenRouteIsConfigured()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            string testUriPath = "/brand/new/path";
            string responseBody = "some body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            EndpointDescription endpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                testUriPath,
                statusCode,
                responseBody);

            IServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);
            serviceConfiguration.AddEndpointDescription(endpointConfiguration);

            using MockWebApiTestServer integrationTestServer = new MockWebApiTestServer(serviceConfiguration);
            HttpClient httpClient = integrationTestServer.CreateHttpClient();
            HttpTestClient httpTestClient = new HttpTestClient(httpClient);

            // Act
            HttpResponseMessage httpResponseMessage = await httpTestClient.SendMessage(testUriPath);

            // Assert
            Assert.NotNull(httpResponseMessage);
            Assert.Equal(statusCode, httpResponseMessage.StatusCode);
            Assert.Equal(responseBody, await httpResponseMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task MockWebApi_ShouldReturnDefaultResonse_WhenRouteIsNotConfigured()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            string testUriPath = "/brand/new/path";
            string unknownTestUriPath = $"{testUriPath}/with/even/more/segments";
            string responseBody = "some body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            DefaultEndpointDescription defaultEndpointConfiguration = EndpointDescriptionFactory.CreateDefaultEndpointDescription();

            EndpointDescription endpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                testUriPath,
                statusCode,
                responseBody);

            IServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);
            serviceConfiguration.DefaultEndpointDescription = defaultEndpointConfiguration;
            serviceConfiguration.AddEndpointDescription(endpointConfiguration);

            using MockWebApiTestServer integrationTestServer = new MockWebApiTestServer(serviceConfiguration);
            HttpClient httpClient = integrationTestServer.CreateHttpClient();
            HttpTestClient httpTestClient = new HttpTestClient(httpClient);

            // Act
            HttpResponseMessage httpResponseMessage = await httpTestClient.SendMessage(unknownTestUriPath);

            // Assert
            Assert.NotNull(httpResponseMessage);
            Assert.Equal(defaultEndpointConfiguration.Result.StatusCode, httpResponseMessage.StatusCode);
            Assert.Equal(defaultEndpointConfiguration.Result.Body, await httpResponseMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task MockWebApi_ShouldRemoveEndpointConfig_WhenLifecycleIsApplyOnce()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            string testUriPath = "/brand/new/path";
            string responseBody = "some body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            DefaultEndpointDescription defaultEndpointConfiguration = EndpointDescriptionFactory.CreateDefaultEndpointDescription();

            EndpointDescription endpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                testUriPath,
                statusCode,
                responseBody);

            endpointConfiguration.LifecyclePolicy = LifecyclePolicy.ApplyOnce;

            IServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);
            serviceConfiguration.DefaultEndpointDescription = defaultEndpointConfiguration;
            serviceConfiguration.AddEndpointDescription(endpointConfiguration);

            using MockWebApiTestServer integrationTestServer = new MockWebApiTestServer(serviceConfiguration);
            HttpClient httpClient = integrationTestServer.CreateHttpClient();
            HttpTestClient httpTestClient = new HttpTestClient(httpClient);

            // Act
            HttpResponseMessage firstHttpResponseMessage = await httpTestClient.SendMessage(testUriPath);
            HttpResponseMessage secondHttpResponseMessage = await httpTestClient.SendMessage(testUriPath);

            // Assert
            Assert.NotNull(firstHttpResponseMessage);
            Assert.Equal(statusCode, firstHttpResponseMessage.StatusCode);
            Assert.Equal(responseBody, await firstHttpResponseMessage.Content.ReadAsStringAsync());

            Assert.NotNull(secondHttpResponseMessage);
            Assert.Equal(defaultEndpointConfiguration.Result.StatusCode, secondHttpResponseMessage.StatusCode);
            Assert.Equal(defaultEndpointConfiguration.Result.Body, await secondHttpResponseMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task MockWebApi_ShouldKeepEndpointConfig_WhenLifecycleIsRepeat()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            string testUriPath = "/brand/new/path";
            string responseBody = "some body";
            HttpStatusCode statusCode = HttpStatusCode.Created;

            DefaultEndpointDescription defaultEndpointConfiguration = EndpointDescriptionFactory.CreateDefaultEndpointDescription();

            EndpointDescription endpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                testUriPath,
                statusCode,
                responseBody);

            endpointConfiguration.LifecyclePolicy = LifecyclePolicy.Repeat;

            IServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);
            serviceConfiguration.DefaultEndpointDescription = defaultEndpointConfiguration;
            serviceConfiguration.AddEndpointDescription(endpointConfiguration);

            using MockWebApiTestServer integrationTestServer = new MockWebApiTestServer(serviceConfiguration);
            HttpClient httpClient = integrationTestServer.CreateHttpClient();
            HttpTestClient httpTestClient = new HttpTestClient(httpClient);

            // Act
            HttpResponseMessage firstHttpResponseMessage = await httpTestClient.SendMessage(testUriPath);
            HttpResponseMessage secondHttpResponseMessage = await httpTestClient.SendMessage(testUriPath);

            // Assert
            Assert.NotNull(firstHttpResponseMessage);
            Assert.Equal(statusCode, firstHttpResponseMessage.StatusCode);
            Assert.Equal(responseBody, await firstHttpResponseMessage.Content.ReadAsStringAsync());

            Assert.NotNull(secondHttpResponseMessage);
            Assert.Equal(statusCode, secondHttpResponseMessage.StatusCode);
            Assert.Equal(responseBody, await secondHttpResponseMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task MockWebApi_ShouldReturnConfguredResponse_WhenJwtTokenIsValid()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            string testUriPath = "/brand/new/path";
            string responseBody = "some body";
            string userName = "TEST-USER-NAME";
            HttpStatusCode statusCode = HttpStatusCode.OK;

            EndpointDescription endpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                testUriPath,
                statusCode,
                responseBody);

            endpointConfiguration.CheckAuthorization = true;
            endpointConfiguration.AllowedUsers = new string[] { userName };

            IServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);
            serviceConfiguration.AddEndpointDescription(endpointConfiguration);
            serviceConfiguration.ErrorResponseEndpointDescription = new DefaultEndpointDescription()
            {
                Result = new HttpResult()
                {
                    Body = "",
                    StatusCode = HttpStatusCode.Unauthorized
                }
            };

            IJwtService jwtService = new JwtService(serviceConfiguration);

            JwtCredentialUser jwtCredentialUser = new JwtCredentialUser()
            {
                Name = userName
            };

            string jwtToken = jwtService.CreateToken(jwtCredentialUser);
            AuthenticationHeaderValue authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", jwtToken);

            using MockWebApiTestServer integrationTestServer = new MockWebApiTestServer(serviceConfiguration);
            HttpClient httpClient = integrationTestServer.CreateHttpClient();
            HttpTestClient httpTestClient = new HttpTestClient(httpClient);

            // Act
            HttpResponseMessage httpResponseMessage = await httpTestClient.SendMessage(testUriPath, authenticationHeaderValue: authenticationHeaderValue);

            // Assert
            Assert.NotNull(httpResponseMessage);
            Assert.Equal(statusCode, httpResponseMessage.StatusCode);
            Assert.Equal(responseBody, await httpResponseMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task MockWebApi_ShouldReturnErrorResponse_WhenJwtTokenIsInvalid()
        {
            // Arrange
            string serviceName = "TEST-SERVICE";
            string testUriPath = "/brand/new/path";
            string responseBody = "some body";
            string authorizedUserName = "TEST-USER-NAME";
            string unauthorizedUserName = "UNAUTHORIZED-USER-NAME";
            HttpStatusCode statusCode = HttpStatusCode.OK;

            EndpointDescription endpointConfiguration = EndpointDescriptionFactory.CreateEndpointDescription(
                testUriPath,
                statusCode,
                responseBody);

            endpointConfiguration.CheckAuthorization = true;
            endpointConfiguration.AllowedUsers = new string[] { authorizedUserName };

            IServiceConfiguration serviceConfiguration = ServiceConfigurationFactory.CreateBaseConfiguration(serviceName);
            serviceConfiguration.AddEndpointDescription(endpointConfiguration);
            serviceConfiguration.ErrorResponseEndpointDescription = new DefaultEndpointDescription()
            {
                Result = new HttpResult()
                {
                    Body = "",
                    StatusCode = HttpStatusCode.Unauthorized
                }
            };

            JwtServiceOptions jwtServiceOptions = new JwtServiceOptions()
            {
                Expiration = TimeSpan.FromMinutes(5),
                SigningKey = "SECRET-SIGNING-KEY",
                Audience = "INTEGRATION-TEST-AUDIENCE",
                Issuer = "INTEGRATION-TEST-ISSUER"
            };

            serviceConfiguration.JwtServiceOptions = jwtServiceOptions;

            IJwtService jwtService = new JwtService(serviceConfiguration);

            JwtCredentialUser jwtCredentialUser = new JwtCredentialUser()
            {
                Name = unauthorizedUserName
            };

            string jwtToken = jwtService.CreateToken(jwtCredentialUser);
            AuthenticationHeaderValue authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", jwtToken);

            using MockWebApiTestServer integrationTestServer = new MockWebApiTestServer(serviceConfiguration);
            HttpClient httpClient = integrationTestServer.CreateHttpClient();
            HttpTestClient httpTestClient = new HttpTestClient(httpClient);

            // Act
            HttpResponseMessage httpResponseMessage = await httpTestClient.SendMessage(testUriPath, authenticationHeaderValue: authenticationHeaderValue);

            // Assert
            Assert.NotNull(httpResponseMessage);
            Assert.Equal(HttpStatusCode.Unauthorized, httpResponseMessage.StatusCode);
            Assert.Empty(await httpResponseMessage.Content.ReadAsStringAsync());
        }

    }
}
