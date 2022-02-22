using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MockWebApi.Configuration;
using MockWebApi.Extension;

namespace MockWebApi.Test
{
    /// <summary>
    /// The ServiceApiTestServer is an embedded MockWebApi server providing the
    /// full feature set of the MockWebApi standalone service, including the
    /// ability to host multiple mocked APIs at once.
    /// </summary>
    public class ServiceApiTestServer : IDisposable
    {

        private readonly TestServer _testServer;
        private ServiceConfigurationProxy _serviceConfigurationProxy;

        public ServiceApiTestServer(IServiceConfiguration serviceConfiguration)
        {
            _serviceConfigurationProxy = new ServiceConfigurationProxy(serviceConfiguration);
            _testServer = CreateTestServer();
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

        private TestServer CreateTestServer()
        {
            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .SetupMockWebApiService()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IServiceConfiguration>(_serviceConfigurationProxy);
                });

            TestServer testServer = new TestServer(hostBuilder);

            return testServer;
        }

    }
}
