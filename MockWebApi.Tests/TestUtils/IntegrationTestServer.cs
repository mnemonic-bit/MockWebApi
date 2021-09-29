using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using MockWebApi.Extension;
using System.Net.Http;

namespace MockWebApi.Tests.TestUtils
{
    internal class IntegrationTestServer
    {

        private readonly TestServer _testServer;

        internal IntegrationTestServer()
        {
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
                .SetupMockWebApi();

            TestServer testServer = new TestServer(hostBuilder);

            return testServer;
        }

    }
}
