using GraphQL.Types;
using MockWebApi.Configuration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.GraphQL
{
    public class HttpResultType : ObjectGraphType<HttpResult>
    {

        public HttpResultType()
        {
            //Field<StringGraphType>("StatusCode"); //TODO: HttpStatusCode is an enum
            Field<StringGraphType>("ContentType");
            Field<StringGraphType>("Body");
        }

    }
}
