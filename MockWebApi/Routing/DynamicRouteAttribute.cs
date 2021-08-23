using System;

namespace MockWebApi.Routing
{
    /// <summary>
    /// This attribute marks a controller class with an initial base route.
    /// This initial route works the same as the RouteAttribute of ASP.Net
    /// does. In addition to that behaviour of setting the base path of a
    /// controller, it also marks the route of the controller to be dynamically
    /// configurable. This enables a controller to be relocatable to a different
    /// base-path at runtime.
    /// </summary>
    public class DynamicRouteAttribute : Attribute
    {

        public DynamicRouteAttribute()
        {
            InitialPath = string.Empty;
        }

        public DynamicRouteAttribute(string initialPath)
        {
            InitialPath = initialPath;
        }

        public string InitialPath { get; }

    }
}
