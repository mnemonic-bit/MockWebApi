using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Routing
{
    /// <summary>
    /// A RouteMatch is the match that hold the match-information
    /// of a path. This information contains the path that was
    /// queried, as well as the EndpointInformation that belongs
    /// to the match, and also the variables and their values that
    /// were bound to the route.
    /// </summary>
    public class RouteMatch<TInfo>
    {

        /// <summary>
        /// Inits a new instance of the RouteMatch class with the required
        /// information about the endpoint and the variable-bindings.
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="variables"></param>
        public RouteMatch(TInfo infos, IDictionary<string, string> variables)
        {
            RouteInformation = infos;
            Variables = variables;
        }

        /// <summary>
        /// Gets the route information of this match, which is the
        /// endpoint information.
        /// </summary>
        public TInfo RouteInformation { get; }

        /// <summary>
        /// Gets the dictionary which contains the variables bound
        /// to this route.
        /// </summary>
        public IDictionary<string, string> Variables { get; }

    }
}
