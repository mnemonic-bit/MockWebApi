using System;
using System.Threading;
using System.Threading.Tasks;
using MockWebApi.Configuration;
using MockWebApi.Extension;

namespace MockWebApi.Service
{
    /// <summary>
    /// This class provides a basic implementation of the <code>IService</code>
    /// interface, and leaves only the implementation of one method which builds
    /// and starts the service.
    /// </summary>
    public abstract class ServiceBase<TConfig> : IService<TConfig>
        where TConfig : IServiceConfiguration
    {

        public ServiceState ServiceState
        {
            get
            {
                return _serviceThread?.ThreadState.GetServiceState() ?? ServiceState.NotStarted;
            }
        }

        public virtual TConfig ServiceConfiguration { get; set; }

        IServiceConfiguration IService.ServiceConfiguration
        {
            get => ServiceConfiguration;
            set
            {
                if (value is TConfig config)
                {
                    ServiceConfiguration = config;
                }
            }
        }

        public ServiceBase(TConfig serviceConfiguration)
        {
            ServiceConfiguration = serviceConfiguration;
            //ServiceConfigurationProxy = new ServiceConfigurationProxy(serviceConfiguration ?? new RestServiceConfiguration("service1", DefaultValues.DEFAULT_MOCK_BASE_URL));
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }

        public void StartService()
        {
            lock (_serviceThreadLock)
            {
                if (_serviceThread != null)
                {
                    throw new InvalidOperationException($"The service thread has already been started, and is in the state '{_serviceThread.ThreadState}'");
                }

                _serviceThread = new Thread(() => ThreadStart(_cancellationTokenSource.Token));
                _serviceThread.IsBackground = true; // mocked interfaces can always be shut down with no warning
                _serviceThread.Start();
            }
        }

        public bool StopService(int millisecondTimeout = 300000)
        {
            lock (_serviceThreadLock)
            {
                if (_serviceThread == null)
                {
                    return false;
                }

                _cancellationTokenSource.Cancel();
                bool threadWasAborted = _serviceThread.Join(millisecondTimeout);

                return threadWasAborted;
            }
        }

        public abstract Task BuildAndStartService(CancellationToken cancellationToken = default);

        public abstract Task StopAndTearDownService(CancellationToken cancellationToken = default);


        //protected ServiceConfigurationProxy ServiceConfigurationProxy { get; init; }

        private Thread? _serviceThread;
        private readonly string _serviceThreadLock = Guid.NewGuid().ToString();
        private readonly CancellationTokenSource _cancellationTokenSource;


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
            catch (AggregateException)
            {
            }
        }

        //TODO: remove this later, we drop the service by cancelling
        // the cancellation token, and then wait for the thread to join.
        private bool ThreadStop(int millisecondTimeout, CancellationToken cancellationToken)
        {
            try
            {
                return StopAndTearDownService(cancellationToken)
                    .Wait(millisecondTimeout, cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (AggregateException)
            {
            }

            return false;
        }

    }
}
