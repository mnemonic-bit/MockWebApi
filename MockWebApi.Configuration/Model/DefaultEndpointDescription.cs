namespace MockWebApi.Configuration.Model
{
    /// <summary>
    /// The DefaultEndpointDescription  is used to describe the default behaviour
    /// of the MockWebApi for requests that are not matched with a configured
    /// EndpointDescription itself.
    /// </summary>
    public class DefaultEndpointDescription
    {

        /// <summary>
        /// Gets or sets whether this endpoint guards with authorization
        /// tokens before the endpoint can be accessed.
        /// </summary>
        public bool CheckAuthorization { get; set; }

        /// <summary>
        /// Gets or sets the list of users allowed to access that endpoint.
        /// </summary>
        public string[] AllowedUsers { get; set; }

        /// <summary>
        /// Holds the list of responses the MockWebApi returns.
        /// TODO: remove the array, make this a single element, then
        /// refactor the whole code according to this change.
        /// </summary>
        public HttpResult Result { get; set; }

        public bool ReturnCookies { get; set; }

    }
}
