namespace MockWebApi.Configuration.Model
{
    /// <summary>
    /// This class is used for reading and writing out the configuration
    /// of the MockWebApi.
    /// </summary>
    public class MockedWebApiServiceConfiguration
    {

        /// <summary>
        /// The name of the service to uniquely identify a service.
        /// If no name was given, the server will choose a GUID as the
        /// name.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// The URL this service will bind to, which may include
        /// the scheme, IP address, and port number.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The default values for the mocked endpoint including the
        /// HTTP result which will be returned by default.
        /// </summary>
        public DefaultEndpointDescription DefaultEndpointDescription { get; set; }

        /// <summary>
        /// A flag which is used to set the server into tracking mode, i.e. all
        /// calls to the service API will also be written to the request-history.
        /// </summary>
        public bool? TrackServiceApiCalls { get; set; }

        /// <summary>
        /// A flag which lets the service log each request to the service API to
        /// the console, similar to what the mocked API does for mocked endpoints.
        /// </summary>
        public bool? LogServiceApiCalls { get; set; }

        /// <summary>
        /// Options describing basic parameters needed for JWT generation.
        /// This options include the secret to be used when new tokens are
        /// generated.
        /// </summary>
        public JwtServiceOptions JwtServiceOptions { get; set; }

        /// <summary>
        /// An array of endpoint descriptions which define what set of URLs will
        /// trigger a response, and how this response looks like.
        /// </summary>
        public EndpointDescription[] EndpointDescriptions { get; set; }

    }
}