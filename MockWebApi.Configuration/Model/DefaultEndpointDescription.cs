namespace MockWebApi.Configuration.Model
{
    /// <summary>
    /// The DefaultEndpointDescription  is used to describe the default behaviour
    /// of the MockWebApi for requests that are not matched with a configured
    /// EndpointDescription itself.
    /// </summary>
    public class DefaultEndpointDescription : BaseEndpointDescription
    {

        /// <summary>
        /// Holds the response that is sent back when no route
        /// is matched.
        /// </summary>
        public HttpResult Result { get; set; }

    }
}
