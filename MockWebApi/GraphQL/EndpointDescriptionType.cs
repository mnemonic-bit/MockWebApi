using GraphQL.Types;
using MockWebApi.Configuration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
