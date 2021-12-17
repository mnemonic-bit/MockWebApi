using GraphQL.Types;

using MockWebApi.Data;

namespace MockWebApi.GraphQL
{
    public class RequestHistorySchema : Schema
    {

        public RequestHistorySchema(IRequestHistory requestHistory)
        {
            Query = new RequestHistoryItemQuery(requestHistory);
        }

    }
}
