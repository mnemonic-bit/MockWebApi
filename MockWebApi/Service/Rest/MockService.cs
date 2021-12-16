using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MockWebApi.Configuration;
using MockWebApi.Extension;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MockWebApi.Service.Rest
{
    /// <summary>
    /// The MockService is the component which represents an instance
    /// of the MockWebApi. Since this mock-server can run multiple
    /// instances at the same time, we encapsulated this in a class
    /// of its own.
    /// </summary>
    public class MockService : IService
    {

        private readonly IHostBuilder _hostBuilder;
        private Thread _serviceThread;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly ServiceConfigurationProxy _serviceConfigurationProxy;

        public ServiceState ServiceState
        {
            get
            {
                return _serviceThread?.ThreadState.GetServiceState() ?? ServiceState.NotStarted;
            }
        }

        public IServiceConfiguration ServiceConfiguration
        {
            get
            {
                return _serviceConfigurationProxy.BaseConfiguration;
            }
            set
            {
                _serviceConfigurationProxy.BaseConfiguration = value;
            }
        }

        public MockService(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
            _serviceConfigurationProxy = new ServiceConfigurationProxy(new ServiceConfiguration());
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void StartService()
        {
            _serviceThread = new Thread(() => ThreadStart(_cancellationTokenSource.Token));
            _serviceThread.IsBackground = true; // mocked interfaces can always be shut down with no warning
            _serviceThread.Start();
        }

        public bool StopService(int millisecondTimeout = 300000)
        {
            _cancellationTokenSource.Cancel();
            bool threadWasAborted = _serviceThread.Join(millisecondTimeout);
            return threadWasAborted;
        }

        private void ThreadStart(CancellationToken cancellationToken)
        {
            try
            {
                Task task = BuildAndStartService(cancellationToken);
                task.Wait();
            }
            catch (OperationCanceledException)
            {

            }
        }

        private async Task BuildAndStartService(CancellationToken cancellationToken = default)
        {
            await _hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IServiceConfiguration>(_serviceConfigurationProxy);
                })
                .Build()
                .RunAsync(cancellationToken);
        }

    }
}
