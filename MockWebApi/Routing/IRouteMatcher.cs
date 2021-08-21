

namespace MockWebApi.Routing
{
    public interface IRouteMatcher<TInfo>
    {

        public void AddRoute(string routeTemplate, TInfo routeInfo);

        public bool Remove(string routeTemplate);

        public bool TryMatch(string route, out TInfo routeInfo);
        
    }
}