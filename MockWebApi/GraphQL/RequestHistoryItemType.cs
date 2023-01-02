using GraphQL.Types;
using MockWebApi.Data;

namespace MockWebApi.GraphQL
{
    public class RequestHistoryItemType : ObjectGraphType<RequestHistoryItem>
    {

        public RequestHistoryItemType()
        {
            Name = nameof(RequestHistoryItem);
            
            Field<RequestInformationType>(nameof(RequestHistoryItem.Request));
            Field<HttpResultType>(nameof(RequestHistoryItem.Response));
        }

    }
}
