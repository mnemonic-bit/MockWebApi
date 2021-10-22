using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using MockWebApi.Configuration.Model;
using MockWebApi.Data;
using MockWebApi.Extension;
using MockWebApi.Model;
using MockWebApi.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockWebApi.Middleware
{
    public class WrapHttpContextMiddleware
    {

        private readonly RequestDelegate _nextDelegate;

        private readonly ILogger<StoreRequestDataMiddleware> _logger;

        public WrapHttpContextMiddleware(
            RequestDelegate next,
            ILogger<StoreRequestDataMiddleware> logger)
        {
            _nextDelegate = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            await _nextDelegate(context);

        }

    }
}
