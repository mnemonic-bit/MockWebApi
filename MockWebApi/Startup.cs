using GraphQL.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MockWebApi.Auth;
using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.GraphQL;
using MockWebApi.Middleware;
using MockWebApi.Templating;
using System;
using System.Reflection;

using ServiceConfiguration = MockWebApi.Configuration.ServiceConfiguration;

namespace MockWebApi
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfigurationCollection>(new ConfigurationCollection());
            services.AddSingleton<IRequestHistory>(new RequestHistory());

            services.AddTransient<IConfigurationReader, ConfigurationReader>();
            services.AddTransient<IConfigurationWriter, ConfigurationWriter>();

            services.AddTransient<IServiceConfiguration, ServiceConfiguration>();
            services.AddTransient<IServiceConfigurationReader, ServiceConfigurationReader>();
            services.AddTransient<IServiceConfigurationWriter, ServiceConfigurationWriter>();

            //TODO: change this, change the configuration in general to include this structure,
            // make it changable at runtime.
            services.AddSingleton(new JwtServiceOptions()
            {
                Audience = "AUDIENCE",
                Issuer = "ISSUER",
                Expiration = TimeSpan.FromHours(1),
                SigningKey = "slkdjflskdjflksdjfklsdjflskf" // TODO: configure this?
            });
            services.AddTransient<IJwtService, JwtService>();
            services.AddTransient<IAuthorizationService, AuthorizationService>();

            services.AddTransient<ITemplateExecutor, TemplateExecutor>();
            services.AddTransient<ITemplateParser, TemplateParser>();

            // GraphQL schema types...
            services.AddSingleton<RequestHistorySchema>();

            services.AddControllers();
            services.AddDynamicRouting();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MockWebApi", Version = "v1" });
            });

            services.AddGraphQL()
                .AddGraphTypes(ServiceLifetime.Scoped)
                .AddSystemTextJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MockWebApi v1"));
            }

            app.UseRouting();

            app.UseMiddleware<StoreRequestDataMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();

            app.UseDynamicRouting();

            app.UseAuthorization();

            string configurationFileName = Configuration.GetValue<string>("ServiceConfigurationFileName", "MockWebApiConfiguration.yml");
            app.LoadServiceConfiguration(configurationFileName, false);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGraphQL<RequestHistorySchema>("graphql");
                endpoints.MapGraphQLPlayground("playground");

                endpoints.MapControllerRoute(
                    name: "some-route-name",
                    pattern: "{**slug}",
                    defaults: new { controller = "MockWebApi", action = "MockResults" });
            });

            WriteBanner(logger);
        }

        private void WriteBanner(ILogger<Startup> logger)
        {
            logger.LogInformation($"MockWebApi service version {GetVersion()} has been configured.\n");
        }

        private Version GetVersion()
        {
            Assembly thisAssembly = Assembly.GetAssembly(typeof(Startup));

            Version assemblyVersion = thisAssembly.GetName().Version;

            return assemblyVersion;
        }

    }
}
