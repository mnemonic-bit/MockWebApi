using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MockWebApi.Configuration;
using MockWebApi.Extension;

namespace MockWebApi.Test
{
    /// <summary>
    /// The MockWebApiTestServer is capable of mocking a single API endpoint.
    /// </summary>
    public class MockWebApiTestServer : IDisposable
    {

        private readonly TestServer _testServer;
        private readonly ServiceConfigurationProxy _serviceConfigurationProxy;

        public MockWebApiTestServer(IServiceConfiguration serviceConfiguration)
        {
            _serviceConfigurationProxy = new ServiceConfigurationProxy(serviceConfiguration);
            _testServer = CreateTestServer(_serviceConfigurationProxy);
        }

        public void Dispose()
        {
            if (_testServer != null)
            {
                _testServer.Dispose();
            }
        }

        public HttpClient CreateHttpClient()
        {
            return _testServer.CreateClient();
        }

        public HttpMessageHandler CreateHttpMessageHandler()
        {
            return _testServer.CreateHandler();
        }

        private TestServer CreateTestServer(ServiceConfigurationProxy serviceConfiguration)
        {
            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .SetupMockWebApi()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IServiceConfiguration>(serviceConfiguration);
                });

            TestServer testServer = new TestServer(hostBuilder);

            return testServer;
        }

    }
}
