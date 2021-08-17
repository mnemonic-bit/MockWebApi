using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace MockWebApi.Model
{
    public class RequestInformation
    {

        public string Path { get; set; }

        public string Uri { get; set; }

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
                + $"  Date: {Date}"
                + $"  Path: {Path}"
                + $"  Uri: {Uri}"
                + $"  Content Type: {ContentType}"
                + $"  Body:\n{IndentLines(Body, "    ")}";

            return result;
        }

        private string IndentLines(string lines, string indention)
        {
            return string.Join("\n", lines.Split(new string[] { "\n\r", "\n", "\r" }, StringSplitOptions.None).Select(l => $"{indention}{l}"));
        }

    }
}

/**
Request
  CollectionName: Organization
  Url: http://fake-service:5000/api/organization?subaddressId=123-543298-ab-2384
  HttpVerb: GET
  Date: 2021-07-23T11:09:32
  Path: /api/organization
  Parameters:
  - Key: SubaddressId
    Value: 123-543298-ab-2384
  Cookies: ....
  HttpHeaders:
  - Key: ReturnTo
    Value: EAdaptorHttp
  - Key: x-Orig-Ip
    Value: 10.120.0.5
  Body: |-
    {
      Shipment: {
        shipmentId: 1,
        Containers: [
            ...
        ]
      }
    }
 */