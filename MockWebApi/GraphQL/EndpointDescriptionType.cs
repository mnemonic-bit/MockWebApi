using GraphQL.Types;
using MockWebApi.Configuration.Model;

namespace MockWebApi.GraphQL
{
    public class EndpointDescriptionType : ObjectGraphType<EndpointDescription>
    {

        public EndpointDescriptionType()
        {
            Field(x => x.Route);
            Field(x => x.RequestBodyType);
            Field(x => x.CheckAuthorization);
        }

    }
}
