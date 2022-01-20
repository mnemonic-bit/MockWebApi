using Microsoft.AspNetCore.Hosting;

using MockWebApi.Configuration;
using MockWebApi.Service.Rest;

using Serilog;

namespace MockWebApi.Extension
{
    public static class IWebHostBuilderExtensions
    {

        public static IWebHostBuilder SetupMockWebApi(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder
                .UseStartup<MockServiceStartup>()
                .UseSerilog();

            return webHostBuilder;
        }

        public static IWebHostBuilder SetupMockWebApiService(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder
                .UseStartup<Startup>()
                .UseUrls(HostConfiguration.DEFAULT_HOST_IP_AND_PORT) //TODO: configure this from outside config-sources
                .UseSerilog();

            return webHostBuilder;
        }

    }
}
