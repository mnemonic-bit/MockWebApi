using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MockWebApi.Service
{
    /// <summary>
    /// The MockService is the component which represents an instance
    /// of the MockWebApi. Since this mock-server can run multiple
    /// instances at the same time, we encapsulated this in a class
    /// of its own.
    /// </summary>
    public class MockService
    {

        private readonly IHostBuilder _hostBuilder;
        private Thread _serviceThread;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public MockService(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void StartService()
        {
            _serviceThread = new Thread(() => ThreadStart(_cancellationTokenSource.Token));
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
                .Build()
                .RunAsync(cancellationToken);
        }

    }
}
