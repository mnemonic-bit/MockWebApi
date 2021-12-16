using System.Collections.Generic;

namespace MockWebApi.Configuration.Model
{
    /// <summary>
    /// An EndpointDescription contains all configurable details of a mocked
    /// endpoint (URL) on the MockWebApi server.
    /// </summary>
    public class EndpointDescription : DefaultEndpointDescription
    {

        /// <summary>
        /// Gets or sets the route template for this endpoint
        /// description.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Gets or sets the HTTP-method for this endpoint. If set null,
        /// then all methods are allowed. The default value is null.
        /// </summary>
        public string HttpMethod { get; set; }

        public Dictionary<string, string> Parameters { get; set; }

        public string RequestBodyType { get; set; } // PLAIN_TEXT, YAML, JSON, XML; TODO: make this either a MIME type as string / class, or enum

        public LifecyclePolicy LifecyclePolicy { get; set; }

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
}
