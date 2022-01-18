using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MockWebApi.Configuration;
using MockWebApi.Extension;

using System;
using System.Net.Http;

namespace MockWebApi.Tests.TestUtils
{
    internal class ServiceApiTestServer : IDisposable
    {

        private readonly TestServer _testServer;
        private ServiceConfigurationProxy _serviceConfigurationProxy;

        internal ServiceApiTestServer(IServiceConfiguration serviceConfiguration)
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
