using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MockWebApi.Service
{
    public class LifetimeEventsHostedService : IHostedService
    {

        public LifetimeEventsHostedService(
            ILogger<LifetimeEventsHostedService> logger,
            IHostApplicationLifetime applicationLifetime,
            IHostService hostService)
        {
            _logger = logger;
            _appLifetime = applicationLifetime;
            _hostService = hostService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private readonly ILogger<LifetimeEventsHostedService> _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IHostService _hostService;

        private void OnStarted()
        {
            WriteBanner();
        }

        private void OnStopping()
        {
            foreach(var serviceName in _hostService.ServiceNames)
            {
                //TODO: work on this, because in multi-threaded environments
                // we might get in the situation where the service cannot
                // be found any longer, which will be valid.
                if(!_hostService.TryGetService(serviceName, out IService? service))
                {
                    throw new Exception(); //TODO: change this to a more concrete exception
                }

                service?.StopService();
                _hostService.RemoveService(serviceName);
            }
        }

        private void OnStopped()
        {
        }

        private void WriteBanner()
        {
            _logger.LogInformation($"MockWebApi service version { GetVersion() } has been started.\n");
        }

        private Version GetVersion()
        {
            Assembly? thisAssembly = Assembly.GetAssembly(typeof(Startup));
            Version? assemblyVersion = thisAssembly?.GetName()?.Version;
            return assemblyVersion ?? new Version();
        }

    }
}
