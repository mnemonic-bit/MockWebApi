using System.Collections.Generic;

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

        public bool TryMatch(string route, out RouteMatch<TInfo> routeMatch)
        {
            routeMatch = default(RouteMatch<TInfo>);

            if (_routeMap.TryGetValue(route, out TInfo info))
            {
                routeMatch = new RouteMatch<TInfo>(info, null);
                return true;
            }

            return false;
        }

    }
}
