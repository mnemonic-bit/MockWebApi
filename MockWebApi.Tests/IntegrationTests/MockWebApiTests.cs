using MockWebApi.Auth;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Tests.TestUtils;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace MockWebApi.IntegrationTests.Tests
{
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

            MockWebApiTestServer integrationTestServer = new MockWebApiTestServer(serviceConfiguration);
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

            MockWebApiTestServer integrationTestServer = new MockWebApiTestServer(serviceConfiguration);
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

            MockWebApiTestServer integrationTestServer = new MockWebApiTestServer(serviceConfiguration);
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

            IJwtService jwtService = new JwtService(serviceConfiguration);

            JwtCredentialUser jwtCredentialUser = new JwtCredentialUser()
            {
                Name = unauthorizedUserName
            };

            string jwtToken = jwtService.CreateToken(jwtCredentialUser);
            AuthenticationHeaderValue authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", jwtToken);

            MockWebApiTestServer integrationTestServer = new MockWebApiTestServer(serviceConfiguration);
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
