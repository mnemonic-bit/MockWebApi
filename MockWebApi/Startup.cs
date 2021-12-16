using GraphQL.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MockWebApi.Extension;
using MockWebApi.GraphQL;
using MockWebApi.Middleware;
using System;
using System.Reflection;

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
            // Service-API dependencies
            services.AddMockHostServices();

            // GraphQL schema types...
            services.AddSingleton<RequestHistorySchema>();

            services.AddGraphQL()
                .AddGraphTypes(ServiceLifetime.Scoped)
                .AddSystemTextJson();

            // The service-controller
            services.AddControllers();
            services.AddDynamicRouting();

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MockWebApi", Version = "v1" });
                c.AddServer(new OpenApiServer() { Url = "http://0.0.0.0:5000" });
                c.AddServer(new OpenApiServer() { Url = "http://0.0.0.0:6000" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MockWebApi v1"));

            app.UseRouting();

            //app.UseMiddleware<TimeMeasurementMiddleware>();
            //app.UseMiddleware<StoreRequestDataMiddleware>();
            //app.UseMiddleware<LoggingMiddleware>();

            //app.UseDynamicRouting();

            app.UseAuthorization();

            string configurationFileName = Configuration.GetValue<string>("ServiceConfigurationFileName", "MockWebApiConfiguration.yml");
            app.LoadServiceConfiguration(configurationFileName, false);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGraphQL<RequestHistorySchema>("graphql");
                endpoints.MapGraphQLPlayground("playground");
            });

            WriteBanner(logger);
        }

        private void WriteBanner(ILogger<Startup> logger)
        {
            logger.LogInformation($"MockWebApi service version {GetVersion()} has been started.\n");
        }

        private Version GetVersion()
        {
            Assembly thisAssembly = Assembly.GetAssembly(typeof(Startup));

            Version assemblyVersion = thisAssembly.GetName().Version;

            return assemblyVersion;
        }

    }
}
