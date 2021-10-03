using GraphQL.Types;
using MockWebApi.Configuration.Model;

namespace MockWebApi.GraphQL
{
    public class EndpointDescriptionQuery : ObjectGraphType
    {

        public EndpointDescriptionQuery()
        {
            Field<ListGraphType<EndpointDescriptionType>>(
                "endpoints",
                resolve: context => new object[] { new EndpointDescription() { Route = "/some/test/path", RequestBodyType = "text/plain", CheckAuthorization = true } });
        }

    }
}
