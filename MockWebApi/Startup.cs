using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockWebApi.Extension;
using MockWebApi.GraphQL;
using MockWebApi.Middleware;

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
            // "self"-Service-API dependencies
            services.AddMockHostServices();

            // GraphQL dependencies
            services.AddGraphQLServices();

            // The service-controller
            services.AddControllers();
            services.AddDynamicRouting();

            // Swagger services
            services.AddSwagger();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            // We do not use this, because this is app is a testing-device
            // which should not make our lives easier by coercing any user
            // into using a certain protocol. Using HTTP for debugging of
            // connectivity issues is after all a valid use-case.
            //app.UseHttpsRedirection();

            app.UseMiddleware<ExceptionLoggerMiddleware>();

            //app.UseSwagger();
            //app.UseSwagger(c => c.RouteTemplate = "/swagger/v1/swagger.json" );
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MockWebApi v1");
                //c.SwaggerEndpoint("/api/demo-service/swagger/v1/swagger.json", "DemoService v1");
            });

            app.UseRouting();

            app.UseMiddleware<TimeMeasurementMiddleware>();

            app.UseAuthorization();

            string configurationFileName = Configuration.GetValue("ServiceConfigurationFileName", "MockWebApiConfiguration.yml") ?? "MockWebApiConfiguration.yml";
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
