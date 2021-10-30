using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Service
{
    /// <summary>
    /// The HostService is the central class which holds together
    /// all the references for each service (for mocking REST APIs,
    /// or for proxying, gRPC) and makes the instance which is used
    /// to servce the requests available through an interface.
    /// </summary>
    public class HostService : IHostService
    {

        private readonly IDictionary<string, IService> _services;

        public HostService()
        {
            _services = new Dictionary<string, IService>();
        }

        public void AddService(string serviceName, IService service)
        {
            _services.Add(serviceName, service);
        }

        public bool RemoveService(string serviceName)
        {
            return _services.Remove(serviceName);
        }

        public bool TryGetService(string serviceName, out IService service)
        {
            return _services.TryGetValue(serviceName, out service);
        }

    }
}
