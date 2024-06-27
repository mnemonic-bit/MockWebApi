using System.Collections.Generic;
using System.Linq;

namespace MockWebApi.Routing
{
    /// <summary>
    /// This is a simple route matcher wich can only match constants. Its an alternative
    /// to the full <code>RouteGraphMatcher</code> for testing-purposes.
    /// </summary>
    /// <typeparam name="TInfo"></typeparam>
    public class SimpleRouteMatcher<TInfo> : IRouteMatcher<TInfo>
    {

        private readonly IDictionary<string, TInfo> _routes;

        public SimpleRouteMatcher()
        {
            _routes = new Dictionary<string, TInfo>();
        }

        public void AddRoute(string routeTemplate, TInfo routeInfo)
        {
            _routes.Add(routeTemplate, routeInfo);
        }

        public bool ContainsRoute(string routeTemplate)
        {
            return _routes.ContainsKey(routeTemplate);
        }

        public bool TryFindRoute(string path, out TInfo? info)
        {
            return _routes.TryGetValue(path, out info);
        }

        public IEnumerable<TInfo> GetAllRoutes()
        {
            return _routes.Values.AsEnumerable();
        }

        public bool Remove(string routeTemplate)
        {
            return _routes.Remove(routeTemplate);
        }

        public void RemoveAll()
        {
            _routes.Clear();
        }

        public bool TryMatch(string path, out RouteMatch<TInfo>? routeMatch)
        {
            routeMatch = default;

            if (!TryFindRoute(path, out TInfo? info) || info == null)
            {
                return false;
            }

            routeMatch = new RouteMatch<TInfo>(info, new Dictionary<string, string>(), new Dictionary<string, string>());

            return true;
        }

    }
}
