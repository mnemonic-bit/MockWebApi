using GraphQL.Types;
using MockWebApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.GraphQL
{
    public class RequestHistoryItemQuery : ObjectGraphType
    {

        public RequestHistoryItemQuery(IRequestHistory history)
        {
            Name = "RequestHistory";

            Field<ListGraphType<RequestHistoryItemType>>(
                "requestHistory",
                resolve: context => history.GetAllInformation(null));
        }

    }
}
