using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Routing
{
    public interface IRouteMatcher<TInfo>
    {

        void AddRoute(string routeTemplate, TInfo routeInfo);

        bool Remove(string routeTemplate);

        bool TryMatch(string route, out TInfo routeInfo);

    }
}
