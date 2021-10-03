using GraphQL.Types;
using MockWebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.GraphQL
{
    public class RequestInformationType : ObjectGraphType<RequestInformation>
    {

        public RequestInformationType()
        {
            Field<StringGraphType>("HttpVerb");
            Field<StringGraphType>("Scheme");
            Field<StringGraphType>("Path");
            Field<StringGraphType>("Uri");
            Field<StringGraphType>("Body");
        }

    }
}
