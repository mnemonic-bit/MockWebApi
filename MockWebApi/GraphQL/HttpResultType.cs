using GraphQL.Types;

using MockWebApi.Configuration.Model;

namespace MockWebApi.GraphQL
{
    public class HttpResultType : ObjectGraphType<HttpResult>
    {

        public HttpResultType()
        {
            //Field<DictionaryGraphType>(nameof(HttpResult.Headers)); // Dictionary?
            Field<HttpStatusCodeEnumType>(nameof(HttpResult.StatusCode)); // Enum
            Field<BooleanGraphType>(nameof(HttpResult.IsMockedResult));
            Field<StringGraphType>(nameof(HttpResult.Body));
            Field<StringGraphType>(nameof(HttpResult.ContentType));
            //Field<DictionaryGraphType>(nameof(HttpResult.Cookies)); // Dictionary?
        }

    }
}
