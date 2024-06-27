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
    public class MockGprcHostBuilder
    {

        public static IHostBuilder Create(
            string baseUrls = DefaultValues.DEFAULT_MOCK_BASE_URL,
            string environment = DefaultValues.DEFAULT_HOSTING_ENVIRONMENT_NAME)
        {
            string[] args = new string[] { };

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                .CreateLogger();

            return Host
                .CreateDefaultBuilder(args)
                .UseEnvironment(environment)
                .ConfigureLogging(logBuilder =>
                {
                    logBuilder.AddConsole();
                })
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(options => { options.AddServerHeader = false; })
                        .UseUrls(baseUrls)
                        .SetupMockGrpcApi();
                });
        }

    }
}
