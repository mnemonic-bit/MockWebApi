using MockWebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Data
{
    public interface IRouteConfigurationStore
    {

        void Add(EndpointDescription config);

        bool Remove(string route);

        bool TryGet(string route, out EndpointDescription config);

        EndpointDescription[] GetAllRoutes();

    }
}
