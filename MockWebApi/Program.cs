using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MockWebApi.Extension;
using MockWebApi.Service;
using Serilog;
using Serilog.Filters;
using System.Threading;
using System.Threading.Tasks;

namespace MockWebApi
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
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
                        .UseUrls("http://0.0.0.0:6000") //TODO make this dynamic
                        .SetupMockWebApiService();
                });
        }

    }
}
