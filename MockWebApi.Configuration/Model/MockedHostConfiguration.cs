namespace MockWebApi.Configuration.Model
{
    /// <summary>
    /// This class is used for reading and writing out the configuration
    /// of the MockWebApi.
    /// </summary>
    public class MockedHostConfiguration
    {

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
        /// An array of mocked-service configurations.
        /// </summary>
        public MockedRestServiceConfiguration[] Services { get; set; }

    }
}