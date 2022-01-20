using GraphQL.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using MockWebApi.Configuration;
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
            //services.AddMvc();
            services.AddControllers();
            services.AddDynamicRouting();

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MockWebApi", Version = "v1" });
                //c.AddServer(new OpenApiServer() { Url = MockHostBuilder.DEFAULT_MOCK_BASE_URL });
                c.AddServer(new OpenApiServer() { Url = HostConfiguration.DEFAULT_HOST_IP_AND_PORT });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseMiddleware<ExceptionLoggerMiddleware>();

            app.UseSwagger();
            //app.UseSwagger(c => c.RouteTemplate = "/swagger/v1/swagger.json" );
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MockWebApi v1"));

            app.UseRouting();

            app.UseMiddleware<TimeMeasurementMiddleware>();
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
        }

    }
}
