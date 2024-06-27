using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockWebApi.Auth;
using MockWebApi.Configuration;
using MockWebApi.Data;
using MockWebApi.Middleware;
using MockWebApi.Templating;

namespace MockWebApi.Service.Rest
{
    public class MockRestServiceStartup
    {

        public MockRestServiceStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IRequestHistory>(new RequestHistory());

            services.AddTransient<IServiceConfigurationReader, ServiceConfigurationReader>();
            services.AddTransient<IServiceConfigurationWriter, ServiceConfigurationWriter>();

            services.AddTransient<IConfigurationWriter, ConfigurationWriter>();

            services.AddTransient<IJwtService, JwtService>();
            services.AddTransient<IAuthorizationService, AuthorizationService>();

            services.AddTransient<ITemplateExecutor, TemplateExecutor>();
            services.AddTransient<ITemplateParser, TemplateParser>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<MockRestServiceStartup> logger)
        {
            app.UseMiddleware<TimeMeasurementMiddleware>();
            app.UseMiddleware<StoreRequestDataMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();
            app.UseMiddleware<HttpHeadersMiddleware>(HttpHeadersPolicy.Empty);
            app.UseMiddleware<MockedRestServiceMiddleware>();
        }

    }
}
