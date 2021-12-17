using GraphQL.Types;

using MockWebApi.Data;

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
