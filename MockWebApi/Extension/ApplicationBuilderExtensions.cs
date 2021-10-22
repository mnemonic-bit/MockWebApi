using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
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
            IServiceConfigurationReader serviceConfigurationReader = app.ApplicationServices.GetService<IServiceConfigurationReader>();

            if (!TryReadingFile(configFileName, out string configFileContents))
            {
                if (!required)
                {
                    serviceConfigurationReader.ConfigureService(new MockedWebApiServiceConfiguration());
                }
                return required ? throw new FileNotFoundException($"Configuration file not found ('{configFileName}').") : app;
            }

            IConfigurationReader configurationReader = app.ApplicationServices.GetService<IConfigurationReader>();

            MockedWebApiServiceConfiguration serviceConfiguration = configurationReader.ReadFromYaml(configFileContents);

            serviceConfigurationReader.ConfigureService(serviceConfiguration);

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
