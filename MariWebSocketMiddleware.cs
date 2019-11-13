using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MariSocketMiddleware
{
    public class MariWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceCollection _services;
        private readonly ILogger<MariWebSocketMiddleware> _logger;

        public MariWebSocketMiddleware(
            RequestDelegate next, IServiceCollection services, ILogger<MariWebSocketMiddleware> logger)
        {
            _next = next;
            _services = services;
            _logger = logger;
        }

        #region Invoke

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            };

            _logger.LogInformation("Received a WebSocket request.");
            _logger.LogTrace($"Incoming WebSocket request from " +
                $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}.");
            await HandleBeforeSocketAsync(context);
        }

        #endregion Invoke

        #region HandleBeforeSocketAsync

        private async Task HandleBeforeSocketAsync(HttpContext context)
        {
            var services = _services
                .Where(a => a.ServiceType.BaseType == typeof(MariWebSocketService))
                .Select(a => (MariWebSocketService)a.ImplementationInstance);

            var socketService = services
                .Where(a => a.Path.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (socketService == null)
            {
                _logger.LogInformation($"No WebSocketService found for the request path:" +
                    $" \"{context.Request.Path}\".");
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            _logger.LogInformation("Trying authorize the request...");
            if (!await socketService.AuthorizeAsync(context))
            {
                _logger.LogInformation("The WebSocketSerivce returned unauthorized for that request.");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            _logger.LogInformation("Authorized the request by the WebSocketService.");
            _logger.LogTrace($"Accepting the WebSocket request for " +
                $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}.");

            var nativeSocket = await context.WebSockets.AcceptWebSocketAsync();

            _logger.LogDebug("Successfully accepted the WebSocket request.");

            await HandleAfterSocketAsync(nativeSocket);
        }

        #endregion HandleBeforeSocketAsync

        #region HandleAfterSocketAsync

        private async Task HandleAfterSocketAsync(WebSocket nativeSocket)
        {
            var socket = new MariWebSocket(nativeSocket);
            _logger.LogDebug($"The new WebSocket has the Id: {socket.Id}");
        }

        #endregion HandleAfterSocketAsync
    }
}