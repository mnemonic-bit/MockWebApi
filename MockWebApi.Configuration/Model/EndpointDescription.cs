using System.Collections.Generic;

namespace MockWebApi.Configuration.Model
{
    public class EndpointDescription
    {

        /// <summary>
        /// Gets or sets the route template for this endpoint
        /// description.
        /// </summary>
        public string Route { get; set; }

        public Dictionary<string, string> Parameters { get; set; }

        public string RequestBodyType { get; set; } // PLAIN_TEXT, YAML, JSON, XML

        /// <summary>
        /// Gets or sets whether this endpoint guards with authorization
        /// tokens before the endpoint can be accessed.
        /// </summary>
        public bool CheckAuthorization { get; set; }

        /// <summary>
        /// Gets or sets the list of users allowed to access that endpoint.
        /// </summary>
        public string[] AllowedUsers { get; set; }

        public HttpResult[] Results { get; set; }

        public LifecyclePolicy LifecyclePolicy { get; set; }

        public bool ReturnCookies { get; set; }

        /// <summary>
        /// Gets or sets whether request-information should be
        /// persisted for each request that is received by the
        /// server.
        /// </summary>
        public bool PersistRequestInformation { get; set; }

        /// <summary>
        /// Gets or sets whether request-informatoin should be logged
        /// for each request that is received by the server.
        /// </summary>
        public bool LogRequestInformation { get; set; } = true;

    }

    public enum LifecyclePolicy { ApplyOnce, Repeat }

}
