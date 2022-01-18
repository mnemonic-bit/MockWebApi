# MockWebApi

The MockWebApi is a REST API which provides mocked responses for REST calls.

The service is configurable via a REST API of its own, running on a separate
port which is 6000.


## Quick Start

### Starting The MockWebApi server

To start the MockWebApi you have to check it out, build it, and then start it,
like this

```
git clone https://github.com/mnemonic-bit/MockWebApi
cd MockWebApi
dotnet build
cd MockWebApi
dotnet run
```

### Configuring The MockWebApi server

The MockWebApi can configure two kinds of responses for any mocked service,
which are:

1. A specific repsonse which will be returned whenever the configured URL matches.
2. A default response which will be returned if no specific configured route was found for a request.

We start our quick-start section with an example in which we will have two specific
routes configured. We also will set the default response to a custom value to
demonstrate how this can be done.

#### Starting And Stopping A Mocked Service

Before we start to configure a service, we need to start that new service. For
this we choose the name `demo-service`, and the service is started with a REST
call without a body to the URL `http://localhost:6000/api/demo-service/start`.

All REST calls to alter the behaviour of a mocked service must contain the name
of that service, which is in our case `demo-service`. The URL of the API of
the MockWebApi service are of the form

```
http://localhost:6000/api/{service-name}/{command}/{parameters...}
```

A mocked service can be stopped by sending a REST call to
`http://localhost:6000/api/demo-service/stop` with an empty body.

#### Configuring a Response For a Specific URI

A specific URI can be configured with a call to the URL

```
http://localhost:6000/api/demo-service/configure/route
```

where the body must contain the description details of the response in YAML format:

```
Route: /defined/route
LifecyclePolicy: Repeat
RequestBodyType: text/plain
CheckAuthorization: false
AllowedUsers: []
Result:
  Headers: 
  StatusCode: 201
  Body: some mocked body contents
  ContentType: plain/text
  Cookies: 
ReturnCookies: true
```

The `Route` field is used for matching the request to the mocked service. In this
example, we use the sub-path `/defined/route`, which will match the URL
`http://localhost:5000/defined/route`

#### Configuring a Default Response

The default response of a mocked service can be configured as well. For this
we send a REST call to

```
http://localhost:6000/api/demo-service/configure/default
```

with the body set to:

```
CheckAuthorization: false
AllowedUsers: []
Result:
  Headers: 
  StatusCode: 202
  IsMockedResult: false
  Body: this is the default-resonse
  ContentType: plain/text
  Cookies: 
ReturnCookies: true
```

