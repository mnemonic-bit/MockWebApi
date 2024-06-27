using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MockWebApi.Service.Rest
{
    public class MockGrpcServiceStartup
    {

        public MockGrpcServiceStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: add gPRC components here
            // We use <PackageReference Include="ServiceStack.Server" Version="6.5.0" />
            // c.f. https://docs.servicestack.net/grpc


            //services.AddSingleton<IRequestHistory>(new RequestHistory());

            //services.AddTransient<IServiceConfigurationReader, ServiceConfigurationReader>();
            //services.AddTransient<IServiceConfigurationWriter, ServiceConfigurationWriter>();

            //services.AddTransient<IConfigurationFileWriter, ConfigurationFileWriter>();

            //services.AddTransient<IJwtService, JwtService>();
            //services.AddTransient<IAuthorizationService, AuthorizationService>();

            //services.AddTransient<ITemplateExecutor, TemplateExecutor>();
            //services.AddTransient<ITemplateParser, TemplateParser>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<MockRestServiceStartup> logger)
        {
            //app.UseMiddleware<TimeMeasurementMiddleware>();
            //app.UseMiddleware<StoreRequestDataMiddleware>();
            //app.UseMiddleware<LoggingMiddleware>();
            //app.UseMiddleware<HttpHeadersMiddleware>(HttpHeadersPolicy.Empty);
            //app.UseMiddleware<MockedRestServiceMiddleware>();
        }

    }
}
