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

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IRouteMatcher<EndpointDescription>>(new RouteMatcher<EndpointDescription>());
            services.AddSingleton<IRouteConfigurationStore>(new RouteConfigurationStore());
            services.AddSingleton<IServerConfiguration>(new ServerConfiguration());

            services.AddSingleton<IDataStore>(new DataStore());

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MockWebApi", Version = "v1" });
            });
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

            app.Use(next => context =>
            {
                logger.LogDebug($"Endpoint before UserRouting(): {context.GetEndpoint()?.DisplayName ?? "(null)"}");
                return next(context);
            });

            // we don't use this
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.Use(next => context =>
            {
                logger.LogDebug($"Endpoint after UseRouting(): {context.GetEndpoint()?.DisplayName ?? "(null)"}");
                return next(context);
            });

            app.UseMiddleware<StoreRequestDataMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();

            app.UseAuthorization();

            // to debug an api-call, just add a break-point here:
            //app.Use(async (context, next) =>
            //{
            //    var endPoint = context.GetEndpoint();
            //    var routes = context.Request.RouteValues;
            //    await next();
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapControllerRoute(
                    name: "some-route-name",
                    pattern: "{**slug}",
                    defaults: new { controller = "MockWebApi", action = "MockResults" });
            });
        }

    }
}
