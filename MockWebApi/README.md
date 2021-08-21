# MockWebApi

The MockWebApi is a Docker container which handles HTTP traffic and
returns a configurable HTTP response. The use-case for this container
is in test-environments where a mocked endpoint is needed instead of
the whole system. By using this container as HTTP endpoint we can
mimick the role of mocked object instances in unit-tests but on a
system level.


# Usage

The Docker container exposes its services on port 5000, and currently
understands only HTTP. By default the HTTP response will have a status
code of 200, with an empty body. No additional cookies are generated,
and cookies from the request will not be copied over to the response.


