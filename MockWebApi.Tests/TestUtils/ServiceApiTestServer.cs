using System.Net.Http;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using MockWebApi.Configuration;
using MockWebApi.Extension;

namespace MockWebApi.Tests.TestUtils
{
    internal class ServiceApiTestServer
    {

        private readonly TestServer _testServer;
        private readonly ServiceConfigurationProxy _serviceConfigurationProxy;

        internal ServiceApiTestServer(IServiceConfiguration serviceConfiguration)
        {
            _serviceConfigurationProxy = new ServiceConfigurationProxy(serviceConfiguration);
            _testServer = CreateTestServer();
        }

        internal HttpClient CreateHttpClient()
        {
            return _testServer.CreateClient();
        }

        internal HttpMessageHandler CreateHttpMessageHandler()
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
