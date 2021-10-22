using MockWebApi.Configuration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Tests.TestUtils
{
    internal class EndpointDescriptionFactory
    {

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
                Result = new HttpResult()
                {
                    ContentType = "application/yaml",
                    StatusCode = httpStatusCode,
                    Body = body
                }
            };

            return endpointDescription;
        }

    }
}
