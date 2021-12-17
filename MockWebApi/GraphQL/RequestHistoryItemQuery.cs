using GraphQL.Types;

using MockWebApi.Data;

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
