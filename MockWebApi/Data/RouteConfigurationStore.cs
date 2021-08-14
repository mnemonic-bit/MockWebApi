using MockWebApi.Model;
using System.Collections.Generic;
using System.Linq;

namespace MockWebApi.Data
{
    internal class RouteConfigurationStore : IRouteConfigurationStore
    {

        private readonly Dictionary<string, EndpointDescription> _endpoints;

        public RouteConfigurationStore()
        {
            _endpoints = new Dictionary<string, EndpointDescription>();
        }

        public void Add(EndpointDescription config)
        {
            _endpoints.Add(config.Route, config);
        }

        public bool Remove(string route)
        {
            return _endpoints.Remove(route);
        }

        public bool TryGet(string route, out EndpointDescription config)
        {
            return _endpoints.TryGetValue(route, out config);
        }

        public EndpointDescription[] GetAllRoutes()
        {
            return _endpoints.Values.ToArray();
        }

    }
}
