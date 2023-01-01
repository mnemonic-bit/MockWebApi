using MockWebApi.Configuration.Model;
using System.Net;

namespace MockWebApi.FunctionalTests.TestUtils
{
    internal static class EndpointDescriptionFactory
    {

        public static DefaultEndpointDescription CreateDefaultEndpointDescription()
        {
            DefaultEndpointDescription defaultEndpointDescription = new DefaultEndpointDescription()
            {
                CheckAuthorization = false,
                Result = new HttpResult()
                {
                    StatusCode = HttpStatusCode.OK,
                    Body = "THIS-IS-A-MOCKED-RESULT"
                }
            };

            return defaultEndpointDescription;
        }

        public static EndpointDescription CreateEndpointDescription(string path, HttpStatusCode httpStatusCode)
        {
            return CreateEndpointDescription(path, httpStatusCode, string.Empty);
        }

        public static EndpointDescription CreateEndpointDescription(string path, HttpStatusCode httpStatusCode, string body)
        {
            EndpointDescription endpointDescription = new EndpointDescription()
            {
                Route = path,
                LifecyclePolicy = LifecyclePolicy.ApplyOnce,
                RequestBodyType = "text/plain",
                Results = new HttpResult[]
                {
                    new HttpResult()
                    {
                        ContentType = "application/yaml",
                        StatusCode = httpStatusCode,
                        Body = body
                    }
                }
            };

            return endpointDescription;
        }

    }
}
