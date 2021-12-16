using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MockWebApi.Configuration;
using MockWebApi.Extension;
using System.Net.Http;

namespace MockWebApi.Tests.TestUtils
{
    internal class MockWebApiTestServer
    {

        private readonly TestServer _testServer;
        private readonly ServiceConfigurationProxy _serviceConfigurationProxy;

        internal MockWebApiTestServer(IServiceConfiguration serviceConfiguration)
        {
            _serviceConfigurationProxy = new ServiceConfigurationProxy(serviceConfiguration);
            _testServer = CreateTestServer(_serviceConfigurationProxy);
        }

        internal HttpClient CreateHttpClient()
        {
            return _testServer.CreateClient();
        }

        internal HttpMessageHandler CreateHttpMessageHandler()
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
