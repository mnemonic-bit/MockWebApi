namespace MockWebApi.Configuration.Model
{
    /// <summary>
    /// This class is used for reading and writing out the configuration
    /// of the MockWebApi.
    /// </summary>
    public class MockedWebApiServiceConfiguration
    {

        public DefaultEndpointDescription DefaultEndpointDescription { get; set; }

        public bool? TrackServiceApiCalls { get; set; }
        public bool? LogServiceApiCalls { get; set; }

        public JwtServiceOptions JwtServiceOptions { get; set; }

        public EndpointDescription[] EndpointDescriptions { get; set; }

    }
}