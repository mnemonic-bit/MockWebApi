using GraphQL.Types;
using MockWebApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.GraphQL
{
    public class RequestHistoryItemType : ObjectGraphType<RequestHistoryItem>
    {

        public RequestHistoryItemType()
        {
            Name = "RequestHistoryItem";

            Field<RequestInformationType>("request");
            Field<HttpResultType>("response");
        }

    }
}
