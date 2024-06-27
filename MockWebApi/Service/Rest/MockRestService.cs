using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MockWebApi.Configuration;

namespace MockWebApi.Service.Rest
{
    public class MockRestService : ServiceBase<IRestServiceConfiguration>
    {

        public MockRestService(IHostBuilder hostBuilder, IRestServiceConfiguration serviceConfiguration)
            : base(serviceConfiguration)
        {
            _hostBuilder = hostBuilder;
        }

        public override async Task BuildAndStartService(CancellationToken cancellationToken = default)
        {
            _host = _hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IServiceConfiguration>(ServiceConfiguration);
                    services.AddSingleton(ServiceConfiguration);
                })
                .Build();

            await _host.RunAsync(cancellationToken);
        }

        public override async Task StopAndTearDownService(CancellationToken cancellationToken = default)
        {
            if (_host == null)
            {
                throw new InvalidOperationException("Cannot stop the service before it has been started.");
            }

            await _host.StopAsync(cancellationToken);
        }


        private readonly IHostBuilder _hostBuilder;
        private IHost? _host;


    }
}
