using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace MockWebApi.Extension
{
    public static class IWebHostBuilderExtensions
    {

        public static IWebHostBuilder SetupMockWebApi(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder
                .UseStartup<Startup>()
                //.UseUrls("https://0.0.0.0:6001;http://0.0.0.0:6000")
                .UseSerilog();

            return webHostBuilder;
        }

    }
}
