using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockWebApi.Auth;
using MockWebApi.Configuration;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Middleware;
using MockWebApi.Templating;
using System;
using System.Reflection;

namespace MockWebApi.Service.Rest
{
    public class MockServiceStartup
    {

        public MockServiceStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<MockServiceStartup> logger)
        {
            app.UseMiddleware<StoreRequestDataMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();
            app.UseMiddleware<MockedRestMiddleware>();
        }

    }
}
