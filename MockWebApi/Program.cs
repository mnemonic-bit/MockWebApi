using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MockWebApi.Extension;
using Serilog;
using Serilog.Filters;

namespace MockWebApi
{
    public class Program
    {

        public static void Main(string[] args)
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
                .ConfigureLogging()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseEnvironment("Development")
                        .SetupMockWebApiService();
                });
        }

    }
}
