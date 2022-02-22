using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Middleware;
using System;
using System.IO;

namespace MockWebApi.Extension
{
    public static class ApplicationBuilderExtensions
    {

        public static IApplicationBuilder UseDynamicRouting(this IApplicationBuilder app)
        {
            app.UseMiddleware<DynamicRoutingMiddleware>();

            return app;
        }

        public static IApplicationBuilder LoadServiceConfiguration(this IApplicationBuilder app, string configFileName, bool required = true)
        {
            IHostConfigurationReader? hostConfigurationReader = app.ApplicationServices.GetService<IHostConfigurationReader>();

            if (hostConfigurationReader == null)
            {
                throw new Exception(); //TODO: 
            }

            if (!TryReadingFile(configFileName, out string? configFileContents) || configFileContents == null)
            {
                if (!required)
                {
                    hostConfigurationReader.ConfigureHost(new MockedHostConfiguration());
                }
                return required ? throw new FileNotFoundException($"Configuration file not found ('{configFileName}').") : app;
            }

            IHostConfigurationFileReader configurationReader = app.ApplicationServices.GetService<IHostConfigurationFileReader>()!;

            MockedHostConfiguration hostConfiguration = configurationReader.ReadFromYaml(configFileContents);

            hostConfigurationReader.ConfigureHost(hostConfiguration);

            return app;
        }

        private static bool TryReadingFile(string fileName, out string? contents)
        {
            contents = null;

            if (!File.Exists(fileName))
            {
                return false;
            }

            contents = File.ReadAllText(fileName);
            return true;
        }

    }
}
