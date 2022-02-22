using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;

namespace MockWebApi.Model
{
    /// <summary>
    /// This class extens the HttpContext to 
    /// </summary>
    internal class ExtendedHttpContext : HttpContext
    {

        public ExtendedHttpContext(HttpContext context)
        {
            _connection = context.Connection;
            _features = context.Features;
            Items = new Dictionary<object, object?>(context.Items);
            _request = context.Request;
            RequestAborted = context.RequestAborted;
            RequestServices = context.RequestServices;
            _response = context.Response;
            Session = context.Session;
            TraceIdentifier = context.TraceIdentifier;
            User = context.User;
            _webSockets = context.WebSockets;
        }

        private ConnectionInfo _connection;

        public override ConnectionInfo Connection => _connection;

        public void SetConnection(ConnectionInfo connectionInfo)
        {
            _connection = connectionInfo;
        }

        private IFeatureCollection _features;

        public override IFeatureCollection Features { get { return _features; } }

        public void SetFeatures(IFeatureCollection features)
        {
            _features = features;
        }

        public override IDictionary<object, object?> Items { get; set; }

        private HttpRequest _request;
        public override HttpRequest Request => _request;

        public void SetRequest(HttpRequest request)
        {
            _request = request;
        }

        public override CancellationToken RequestAborted { get; set; }
        public override IServiceProvider RequestServices { get; set; }

        private HttpResponse _response;

        public override HttpResponse Response => _response;

        public void SetResponse(HttpResponse response)
        {
            _response = response;
        }

        public override ISession Session { get; set; }

        public override string TraceIdentifier { get; set; }

        public override ClaimsPrincipal User { get; set; }

        private WebSocketManager _webSockets;

        public override WebSocketManager WebSockets => _webSockets;

        public void SetWebSockets(WebSocketManager webSockets)
        {
            _webSockets = webSockets;
        }

        public override void Abort()
        {
            throw new NotImplementedException();
        }

    }
}
