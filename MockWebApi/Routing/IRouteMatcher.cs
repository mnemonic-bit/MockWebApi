namespace MockWebApi.Routing
{
    public interface IRouteMatcher<TInfo>
    {

        void AddRoute(string routeTemplate, TInfo routeInfo);

        bool Remove(string routeTemplate);

        bool TryMatch(string route, out RouteMatch<TInfo> routeMatch);

    }
}
