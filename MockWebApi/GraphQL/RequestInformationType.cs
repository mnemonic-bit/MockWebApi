using GraphQL.Types;
using MockWebApi.Model;

namespace MockWebApi.GraphQL
{
    public class RequestInformationType : ObjectGraphType<RequestInformation>
    {

        public RequestInformationType()
        {
            Name = nameof(RequestInformation);

            Field<StringGraphType>(nameof(RequestInformation.Path));
            Field<StringGraphType>(nameof(RequestInformation.Uri));
            Field<StringGraphType>(nameof(RequestInformation.Scheme));
            Field<StringGraphType>(nameof(RequestInformation.HttpVerb));
            Field<DateTimeGraphType>(nameof(RequestInformation.Date));
            //Field<DictionaryGraphType>(nameof(RequestInformation.HttpHeaders)); // Dictionary? C.f. https://github.com/graphql-dotnet/graphql-dotnet/issues/318
            Field<StringGraphType>(nameof(RequestInformation.ContentType));
            Field<StringGraphType>(nameof(RequestInformation.ContentEncoding));
            Field<StringGraphType>(nameof(RequestInformation.Body));
        }

    }
}
