using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Routing
{
    public class RouteMatcher<TInfo> : IRouteMatcher<TInfo>
    {

        private readonly Dictionary<string, TInfo> _routeMap;

        internal RouteMatcher()
        {
            _routeMap = new Dictionary<string, TInfo>();
        }

        public void AddRoute(string routeTemplate, TInfo routeInfo)
        {
            _routeMap[routeTemplate] = routeInfo;
        }

        public bool Remove(string routeTemplate)
        {
            return _routeMap.Remove(routeTemplate);
        }

        public bool TryMatch(string route, out TInfo routeInfo)
        {
            if (_routeMap.TryGetValue(route, out routeInfo))
            {
                return true;
            }

            return false;
        }

    }
}
