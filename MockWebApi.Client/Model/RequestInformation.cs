using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace MockWebApi.Client.Model
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