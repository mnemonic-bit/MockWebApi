using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public MockService(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
        }

        public async Task StartServiceAsync(CancellationToken cancellationToken = default)
        {
            await _hostBuilder
                .Build()
                .RunAsync(cancellationToken);
        }

    }
}
