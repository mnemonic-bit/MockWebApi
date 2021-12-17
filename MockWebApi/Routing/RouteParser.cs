using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using MockWebApi.Extension;

namespace MockWebApi.Routing
{
    public class RouteParser : IRouteParser
    {

        public RouteParser()
        {
        }

        public Route Parse(string url)
        {
            (string path, string parameters) = url.SplitAt(GetSpanFor(url, "?"));

            IEnumerable<Route.Part> parts = GetPartsOfPath(path).ToArray();
            IDictionary<string, string> parameterDictionary = GetParameters(parameters);

            return new Route(parts, parameterDictionary);
        }

        private IEnumerable<Route.Part> GetPartsOfPath(string url)
        {
            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            while (!url.IsNullOrEmpty())
            {
                if (url.StartsWith("/"))
                {
                    url = url.Substring(1);
                }

                if (url.StartsWith("{"))
                {
                    (string part, string rest) = url.SplitAt(url.IndexOf("}") + 1);
                    url = rest;

                    yield return new Route.VariablePart(part.Substring(1, part.Length - 2));
                }
                else
                {
                    (string part, string rest) = url.SplitAt(GetEndOfPart(url));
                    url = rest;

                    yield return new Route.LiteralPart(part);
                }
            }
        }

        private IDictionary<string, string> GetParameters(string parameters)
        {
            IDictionary<string, string> result = new Dictionary<string, string>();

            if (parameters.StartsWith("?"))
            {
                parameters = parameters.Substring(1);
            }

            while (!parameters.IsNullOrEmpty())
            {
                int indexOfAmpersand = GetSpanFor(parameters, "&");
                (string param, string rest) = parameters.SplitAt(indexOfAmpersand);

                parameters = rest;
                if (parameters.StartsWith("&"))
                {
                    parameters = parameters.Substring(1);
                }

                int indexOfEquals = param.IndexOf("=");
                (string name, string value) = param.SplitAt(indexOfEquals);
                value = HttpUtility.UrlDecode(value.Substring(1));

                result.Add(name, value);
            }

            return result;
        }

        private int GetEndOfPart(string url)
        {
            int indexOfSlash = GetSpanFor(url, "/");
            int indexOfQuestionMark = GetSpanFor(url, "?");

            return Math.Min(indexOfSlash, indexOfQuestionMark);
        }

        private int GetSpanFor(string str, string delimiter)
        {
            int index = str.IndexOf(delimiter);
            return index == -1 ? str.Length : index;
        }

    }
}
