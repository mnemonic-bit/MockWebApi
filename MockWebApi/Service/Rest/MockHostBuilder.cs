using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration;
using MockWebApi.Extension;
using Serilog;
using Serilog.Filters;

namespace MockWebApi.Service.Rest
{
    /// <summary>
    /// Provides a host builder for creating the MockService.
    /// </summary>
    public class MockHostBuilder
    {

        public static IHostBuilder Create(string baseUrls = DefaultValues.DEFAULT_MOCK_BASE_URL, string environment = "Development")
        {
            string[] args = new string[] { };

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                .CreateLogger();

            return Host
                .CreateDefaultBuilder(args)
                .ConfigureLogging(logBuilder =>
                {
                    logBuilder.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseEnvironment(environment)
                        .UseUrls(baseUrls)
                        .SetupMockWebApi();
                });
        }

    }
}
