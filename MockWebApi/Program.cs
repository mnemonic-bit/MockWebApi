using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
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
                        .UseStartup<Startup>()
                        //.UseUrls("https://0.0.0.0:6001;http://0.0.0.0:6000")
                        .UseSerilog();
                });
        }

    }
}
