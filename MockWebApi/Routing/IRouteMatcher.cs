using System.Collections.Generic;

namespace MockWebApi.Routing
{
    public interface IRouteMatcher<TInfo>
    {

        void AddRoute(string routeTemplate, TInfo routeInfo);

        bool ContainsRoute(string routeTemplate);

        IEnumerable<TInfo> GetAllRoutes();

        bool Remove(string routeTemplate);

        void RemoveAll();

        bool TryMatch(string path, out RouteMatch<TInfo>? routeMatch);

        bool TryFindRoute(string path, out TInfo? info);

    }
}
