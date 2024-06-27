using System.IO;
using Microsoft.AspNetCore.Builder;
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
    public class MockRestHostBuilder
    {

        public static IHostBuilder Create(
            string baseUrls = DefaultValues.DEFAULT_MOCK_BASE_URL,
            string environment = "Development")
        {
            string[] args = new string[] { };

            //NEW>>
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                ApplicationName = typeof(Program).Assembly.FullName,
                ContentRootPath = Directory.GetCurrentDirectory(),
                EnvironmentName = Environments.Staging,
                WebRootPath = "customwwwroot"
            });

            var app = builder.Build();
            //NEW<<

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
                .UseEnvironment(environment)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(options => { options.AddServerHeader = false; })
                        .UseUrls(baseUrls)
                        .SetupMockRestApi();
                });
        }

    }
}
