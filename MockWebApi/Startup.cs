using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MockWebApi.Data;
using MockWebApi.Middleware;
using MockWebApi.Model;
using MockWebApi.Routing;
using System;

namespace MockWebApi
{
    public class Startup
    {

        private readonly ILogger<Startup> _logger;

        public Startup(ILogger<Startup> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IRouteConfigurationStore>(new RouteConfigurationStore());
            services.AddSingleton<IServerConfiguration>(new ServerConfiguration());

            services.AddSingleton<IDataStore>(new DataStore());
            //services.AddTransient<GenericRouteTransformer>();

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MockWebApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MockWebApi v1"));
            }

            app.Use(next => context =>
            {
                _logger.LogDebug($"Endpoint before UserRouting(): {context.GetEndpoint()?.DisplayName ?? "(null)"}");
                return next(context);
            });

            // we don't use this
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.Use(next => context =>
            {
                _logger.LogDebug($"Endpoint after UseRouting(): {context.GetEndpoint()?.DisplayName ?? "(null)"}");
                return next(context);
            });

            //app.Map("/calculator", random => random.UseMiddleware<DemoMiddleware>());
            //app.Map("/store", random => random.UseMiddleware<StoreRequestDataMiddleware>());
            app.UseMiddleware<StoreRequestDataMiddleware>();

            app.UseAuthorization();

            // to debug an api-call, just add a break-point here:
            //app.Use(async (context, next) =>
            //{
            //    var endPoint = context.GetEndpoint();
            //    var routes = context.Request.RouteValues;
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapControllerRoute(
                    name: "some-route-name",
                    pattern: "{**slug}",
                    defaults: new { controller = "MockWebApi", action = "MockResults" });

                //endpoints.Map().WithMetadata();

                //endpoints.MapDynamicControllerRoute<GenericRouteTransformer>("{**slug}"); // this pattern works as a catch-all for all URLs that are not matched by the routing
            });
        }

    }
}
