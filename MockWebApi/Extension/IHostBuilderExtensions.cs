using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Filters;

namespace MockWebApi.Extension
{
    public static class IHostBuilderExtensions
    {

        public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder, bool enableConsoleLogging = false)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                .CreateLogger();

            if (enableConsoleLogging)
            {
                hostBuilder.ConfigureLogging(logBuilder =>
                {
                    logBuilder.AddConsole();
                });
            }

            return hostBuilder;
        }

    }
}
