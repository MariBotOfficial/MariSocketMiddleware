using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MariSocketMiddleware
{
    /// <summary>
    /// The <see cref="MariWebSocketMiddleware"/>.
    /// </summary>
    public readonly struct MariWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _services;
        private readonly ILogger<MariWebSocketMiddleware> _logger;

        /// <summary>
        /// Create a instance of the <see cref="MariWebSocketMiddleware"/>.
        /// </summary>
        /// <param name="next">The <see cref="RequestDelegate"/></param>
        /// <param name="services">Your <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">An <see cref="ILogger"/> for the <see cref="MariWebSocketMiddleware"/>.</param>
        public MariWebSocketMiddleware(
            RequestDelegate next, IServiceProvider services, ILogger<MariWebSocketMiddleware> logger)
        {
            _next = next;
            _services = services;
            _logger = logger;
        }

        #region Invoke

        /// <summary>
        /// The request handler (you can pass other depencies in the ctor).
        /// </summary>
        /// <param name="context">The request context.</param>
        /// <returns></returns>
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
            var service = _services.GetServices<IMariWebSocketService>()
                .Select(a => a as MariBaseWebSocketService)
                .Where(a => context.Request.Path.Equals(a.Path, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (service.HasNoContent())
            {
                _logger.LogInformation($"No WebSocketService found for the request path:" +
                    $" \"{context.Request.Path}\".");
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            if (service.IsDisposed)
            {
                _logger.LogError(new ObjectDisposedException(nameof(service)),
                    "The server cannot use that service because he's disposed.");
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                return;
            }

            _logger.LogDebug("Trying authorize the request...");
            if (!await service.AuthorizeAsync(context).Try(_logger, service, default))
            {
                _logger.LogInformation("The WebSocketService returned unauthorized for that request.");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            _logger.LogInformation("Authorized the request by the WebSocketService.");
            _logger.LogTrace($"Accepting the WebSocket request for " +
                $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}.");

            var nativeSocket = await context.WebSockets.AcceptWebSocketAsync()
                .Try(_logger, service, default);

            _logger.LogInformation("Successfully accepted the WebSocket request.");

            await HandleAfterSocketAsync(nativeSocket, service, context);
        }

        #endregion HandleBeforeSocketAsync

        #region HandleAfterSocketAsync

        private async Task HandleAfterSocketAsync
            (WebSocket nativeSocket, MariBaseWebSocketService service, HttpContext context)
        {
            var socket = new MariWebSocket(nativeSocket, service.Cts.Token, service);
            _logger.LogDebug($"The new WebSocket has the Id: {socket.Id}");

            service.AddClient(socket);

            await service.OnOpenAsync(socket)
                .Try(_logger, service, socket, false);

            try
            {
                await ReadAsync(socket, service, context);
            }
            catch (TaskCanceledException) { }
        }

        #endregion HandleAfterSocketAsync

        #region ReadAsync

        private async Task ReadAsync
            (MariWebSocket socket, MariBaseWebSocketService service, HttpContext context)
        {
            while (socket.WebSocket.State == WebSocketState.Open)
            {
                var buffer = service.Buffer;
                var result = await socket.WebSocket.ReceiveAsync(buffer, service.Cts.Token)
                    .Try(_logger, service, socket, false);

                if (result.HasNoContent())
                    continue;

                await ReadMessageAsync(result, buffer, service, socket, context);
            }
        }

        #endregion ReadAsync

        #region ReadMessageAsync

        private async Task ReadMessageAsync
            (WebSocketReceiveResult result, byte[] buffer,
            MariBaseWebSocketService service, MariWebSocket socket, HttpContext context)
        {
            if (!result.EndOfMessage)
                return;

            Array.Resize(ref buffer, Array.FindLastIndex(buffer, a => a != 0) + 1);

            _logger.LogTrace($"Incoming WebSocket message from " +
                $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}.");
            _logger.LogDebug($"Received WebSocket message from client's id: {socket.Id}");

            if (result.MessageType.Equals(WebSocketMessageType.Text))
            {
                await service.OnMessageAsync(socket, Encoding.UTF8.GetString(buffer))
                    .Try(_logger, service, socket, false);
            }
            else if (result.MessageType.Equals(WebSocketMessageType.Close))
            {
                _logger.LogTrace($"Incoming WebSocket disconnect from " +
                    $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}.");
                _logger.LogDebug($"WebSocket with id {socket.Id} disconnected.");

                await service.OnDisconnectedAsync(
                    socket, result.CloseStatus.Value, Encoding.UTF8.GetString(buffer))
                    .Try(_logger, service, socket, false);

                service.RemoveClient(socket);

                await socket.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    "Closed by remote", service.Cts.Token)
                    .Try(_logger, service, socket, false);

                socket.Dispose();
            }
        }

        #endregion ReadMessageAsync
    }
}