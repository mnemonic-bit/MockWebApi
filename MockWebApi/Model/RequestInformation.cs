using MockWebApi.Extension;
using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace MockWebApi.Model
{
    public class RequestInformation
    {

        public string Path { get; set; }

        public string Uri { get; set; }

        public bool PathMatchedTemplate { get; set; }

        public string Scheme { get; set; }

        public string HttpVerb { get; set; }

        public DateTime Date { get; set; }

        public Dictionary<string, string> Parameters { get; set; }

        public Dictionary<string, string> Cookies { get; set; }

        public Dictionary<string, string> HttpHeaders { get; set; }

        public string ContentType { get; set; }

        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Body { get; set; }

        public override string ToString()
        {
            string result = "HTTP request:\n"
                + $"  HTTP Verb: {HttpVerb}\n"
                + $"  Date: {Date}\n"
                + $"  Path: {Path}\n"
                + $"  Uri: {Uri}\n"
                + $"  Content Type: {ContentType}\n"
                + $"  Body:\n{Body.IndentLines("    ")}\n";

            return result;
        }

    }
}