using Microsoft.AspNetCore.Hosting;
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
                //.UseUrls("https://0.0.0.0:6001;http://0.0.0.0:6000")
                .UseSerilog();

            return webHostBuilder;
        }

    }
}
