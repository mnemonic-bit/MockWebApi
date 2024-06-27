namespace MockWebApi.Configuration.Model
{
    /// <summary>
    /// This class is used for reading and writing out the configuration
    /// of the MockWebApi.
    /// </summary>
    public class MockedRestServiceConfiguration : MockedServiceConfiguration
    {

        /// <summary>
        /// The default values for the mocked endpoint including the
        /// HTTP result which will be returned by default.
        /// </summary>
        public DefaultEndpointDescription DefaultEndpointDescription { get; set; }

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

        public MockedRestServiceConfiguration()
            : base("REST")
        {
        }

    }
}