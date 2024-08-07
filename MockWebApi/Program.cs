using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MockWebApi.Extension;
using Serilog;

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

            return Host
                .CreateDefaultBuilder(args)
                .UseEnvironment("Development")
                .ConfigureLogging()
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .SetupServiceConfigurationApi();
                });
        }

    }
}
