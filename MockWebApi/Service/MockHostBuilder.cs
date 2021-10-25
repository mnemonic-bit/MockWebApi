using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MockWebApi.Extension;
using Serilog;
using Serilog.Filters;

namespace MockWebApi.Service
{
    /// <summary>
    /// Provides a host builder for creating the MockService.
    /// </summary>
    public class MockHostBuilder
    {

        public static IHostBuilder Create(string[] args)
        {
            if (args == null)
            {
                args = new string[] { };
            }

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
                        .UseEnvironment("Development")
                        .UseUrls("http://0.0.0.0:5000") //TODO make this dynamic
                        .SetupMockWebApi();
                });
        }

    }
}
