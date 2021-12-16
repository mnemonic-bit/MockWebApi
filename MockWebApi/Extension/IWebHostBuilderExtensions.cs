using Microsoft.AspNetCore.Hosting;
using MockWebApi.Service.Rest;
using MockWebApi.Service;
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
                .UseUrls("http://0.0.0.0:6000") // TODO: this should be done via the IServiceConfiguration
                .UseSerilog();

            return webHostBuilder;
        }

    }
}
