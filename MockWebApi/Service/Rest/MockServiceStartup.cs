using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MockWebApi.Auth;
using MockWebApi.Configuration;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Middleware;
using MockWebApi.Templating;
using System;
using System.Reflection;

using ServiceConfiguration = MockWebApi.Configuration.ServiceConfiguration;

namespace MockWebApi.Service.Rest
{
    public class MockServiceStartup
    {

        public MockServiceStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IRequestHistory>(new RequestHistory());

            services.AddTransient<IConfigurationReader, ConfigurationReader>();
            services.AddTransient<IConfigurationWriter, ConfigurationWriter>();

            services.AddTransient<IServiceConfigurationReader, ServiceConfigurationReader>();
            services.AddTransient<IServiceConfigurationWriter, ServiceConfigurationWriter>();

            services.AddTransient<IJwtService, JwtService>();
            services.AddTransient<IAuthorizationService, AuthorizationService>();

            services.AddTransient<ITemplateExecutor, TemplateExecutor>();
            services.AddTransient<ITemplateParser, TemplateParser>();

            services.AddDynamicRouting();

            services.AddControllers(); //TODO: this will be obsolete after we rewrote the routing

            //TODO: add an implementation of this class below to provide
            // Swagger-capabilities to the end-user for the dynamic methods
            // this mock-service provides.
            //Microsoft.AspNetCore.Mvc.ApiExplorer.IApiDescriptionGroupCollectionProvider
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<MockServiceStartup> logger)
        {
            app.UseRouting(); //TODO: this will be replaced as soon as we have our own extended routing incoporated

            app.UseMiddleware<StoreRequestDataMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();

            app.UseDynamicRouting();

            string configurationFileName = Configuration.GetValue("ServiceConfigurationFileName", "MockWebApiConfiguration.yml");
            app.LoadServiceConfiguration(configurationFileName, false);

            app.UseEndpoints(endpoints =>
            {
                //TODO: Use middleware for this instead.
                endpoints.MapControllerRoute(
                    name: "some-route-name",
                    pattern: "{**slug}",
                    defaults: new { controller = "MockWebApi", action = "MockResults" });
            });

            WriteBanner(logger);
        }

        private void WriteBanner(ILogger<MockServiceStartup> logger)
        {
            logger.LogInformation($"MockWebApi service version {GetVersion()} has been configured.\n");
        }

        private Version GetVersion()
        {
            Assembly thisAssembly = Assembly.GetAssembly(typeof(MockServiceStartup));

            Version assemblyVersion = thisAssembly.GetName().Version;

            return assemblyVersion;
        }

    }
}
