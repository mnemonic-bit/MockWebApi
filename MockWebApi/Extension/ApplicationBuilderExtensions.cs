using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MockWebApi.Configuration;
using MockWebApi.Middleware;
using System.IO;

namespace MockWebApi.Extension
{
    public static class ApplicationBuilderExtensions
    {

        public static IApplicationBuilder UseDynamicRouting(this IApplicationBuilder app)
        {
            if (app == null)
            {
                return null;
            }

            app.UseMiddleware<DynamicRoutingMiddleware>();

            return app;
        }

        public static IApplicationBuilder LoadServiceConfiguration(this IApplicationBuilder app, string configFileName, bool required = true)
        {
            IConfigurationReader configurationReader = app.ApplicationServices.GetService<IConfigurationReader>();

            if (!TryReadingFile(configFileName, out string configFileContents))
            {
                if (!required)
                {
                    configurationReader.ConfigureService(new ServiceConfiguration());
                }
                return required ? throw new FileNotFoundException($"Configuration file not found ('{configFileName}').") : app;
            }

            ServiceConfiguration serviceConfiguration = configurationReader.ReadFromYaml(configFileContents);
            configurationReader.ConfigureService(serviceConfiguration);

            return app;
        }

        private static bool TryReadingFile(string fileName, out string contents)
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
