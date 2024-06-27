using Microsoft.AspNetCore.Hosting;
using MockWebApi.Configuration;
using MockWebApi.Service.Rest;

namespace MockWebApi.Extension
{
    public static class IWebHostBuilderExtensions
    {

        public static IWebHostBuilder SetupMockRestApi(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder
                .UseStartup<MockRestServiceStartup>();

            return webHostBuilder;
        }

        public static IWebHostBuilder SetupMockGrpcApi(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder
                .UseStartup<MockGrpcServiceStartup>();

            return webHostBuilder;
        }

        public static IWebHostBuilder SetupServiceConfigurationApi(this IWebHostBuilder webHostBuilder, string baseUrls = HostConfiguration.DEFAULT_HOST_IP_AND_PORT)
        {
            webHostBuilder
                .UseStartup<Startup>()
                .UseKestrel(options => { options.AddServerHeader = false; })
                .UseUrls(baseUrls);

            return webHostBuilder;
        }

    }
}
