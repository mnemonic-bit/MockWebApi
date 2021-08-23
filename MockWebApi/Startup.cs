using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MockWebApi.Configuration;
using MockWebApi.Data;
using MockWebApi.Extension;
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
            services.AddSingleton<IConfigurationCollection>(new ConfigurationCollection());
            services.AddSingleton<IDataStore>(new DataStore());

            services.AddTransient<IConfigurationReader, ConfigurationReader>();
            services.AddTransient<IConfigurationWriter, ConfigurationWriter>();

            services.AddControllers();
            services.AddDynamicRouting();

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

                endpoints.MapControllerRoute(
                    name: "some-route-name",
                    pattern: "{**slug}",
                    defaults: new { controller = "MockWebApi", action = "MockResults" });
            });
        }

    }
}
