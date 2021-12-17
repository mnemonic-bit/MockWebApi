using GraphQL.Types;

using MockWebApi.Model;

namespace MockWebApi.GraphQL
{
    public class RequestInformationType : ObjectGraphType<RequestInformation>
    {

        public RequestInformationType()
        {
            Field<StringGraphType>("Path");
            Field<StringGraphType>("Uri");
            Field<StringGraphType>("Scheme");
            Field<StringGraphType>("HttpVerb");
            Field<DateTimeGraphType>("Date");
            //Field<DictionaryGraphType>("HttpHeaders"); // Dictionary? C.f. https://github.com/graphql-dotnet/graphql-dotnet/issues/318
            Field<StringGraphType>("ContentType");
            Field<StringGraphType>("Body");
        }

    }
}
