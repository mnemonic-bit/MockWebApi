using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MockWebApi.Configuration;

namespace MockWebApi.Service.Grpc
{
    public class MockGrpcService : ServiceBase<IGrpcServiceConfiguration>
    {

        public MockGrpcService(IHostBuilder hostBuilder, IGrpcServiceConfiguration serviceConfiguration)
            : base(serviceConfiguration)
        {
            _hostBuilder = hostBuilder;
        }

        public override async Task BuildAndStartService(CancellationToken cancellationToken = default)
        {
            await _hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IServiceConfiguration>(ServiceConfiguration);
                    services.AddSingleton(ServiceConfiguration);
                })
                .Build()
                .RunAsync(cancellationToken);
        }

        public override Task StopAndTearDownService(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }


        private readonly IHostBuilder _hostBuilder;

    }
}
