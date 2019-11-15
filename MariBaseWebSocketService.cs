using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MariSocketMiddleware
{
    /// <summary>
    /// The base WebSocketService.
    /// </summary>
    public abstract class MariBaseWebSocketService : IMariWebSocketService
    {
        private readonly ConcurrentDictionary<string, MariWebSocket> Sockets;
        internal readonly CancellationTokenSource Cts;

        /// <summary>
        /// Creates a Instance of the base <see cref="MariBaseWebSocketService"/>.
        /// </summary>
        public MariBaseWebSocketService()
        {
            Sockets = new ConcurrentDictionary<string, MariWebSocket>();
            Cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Indicates the buffer to read the WebSockets messages, the default is 512.
        /// </summary>
        public byte[] Buffer { get; protected set; } = new byte[512];

        /// <summary>
        /// Indicates if this service is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <inheritdoc/>
        public abstract string Path { get; set; }

        /// <inheritdoc/>
        public abstract Task<bool> AuthorizeAsync(HttpContext context);

        /// <summary>
        /// Indicates when a WebSocket connection is successfully opened.
        /// </summary>
        /// <param name="webSocket">The <see cref="MariWebSocket"/> connected.</param>
        /// <returns></returns>
        internal abstract Task OnOpenAsync(MariWebSocket webSocket);

        /// <summary>
        /// Indicates when an error occurs.
        /// </summary>
        /// <param name="exception">The Exception throwed.</param>
        /// <param name="socket">The <see cref="MariWebSocket"/> where the error ocurried, can be null.</param>
        /// <returns></returns>
        internal abstract Task OnErrorAsync(Exception exception, MariWebSocket socket);

        /// <summary>
        /// Indicates when a client disconnected.
        /// </summary>
        /// <param name="socket">The WebSocket client disconnected.</param>
        /// <param name="code">The code reason of the disconnect.</param>
        /// <param name="reason">The description of the close reason.</param>
        /// <returns></returns>
        internal abstract Task OnDisconnectedAsync
            (MariWebSocket socket, WebSocketCloseStatus code, string reason);

        /// <summary>
        /// Indicates when a message is received.
        /// </summary>
        /// <param name="socket">The client who sent the message.</param>
        /// <param name="message">The message sent by the client.</param>
        /// <returns></returns>
        internal abstract Task OnMessageAsync(MariWebSocket socket, string message);

        /// <summary>
        /// Send a message to all WebSockets clients.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns></returns>
        protected async Task SendMessageToAllAsync(string message)
        {
            foreach (var client in Sockets.Values)
                await client.SendAsync(message)
                    .Try<MariBaseWebSocketService>(null, this, client, false);
        }

        internal void AddClient(MariWebSocket socket)
            => Sockets.TryAdd(socket.Id, socket);

        internal void RemoveClient(MariWebSocket socket)
            => RemoveClient(socket.Id);

        /// <summary>
        /// Remove the client from this service cache.
        /// Be careful, this method don't close or dispose the WebSocket client,
        /// if you want that just <see cref="MariWebSocket.Dispose"/> him.
        /// </summary>
        /// <param name="id">The <see cref="MariWebSocket.Id"/> to remove from the cache.</param>
        protected void RemoveClient(string id)
            => Sockets.TryRemove(id, out var _);

        /// <summary>
        /// Get All Clients connecteds in this service.
        /// </summary>
        /// <returns>An IReadOnlyCollection with all clients connecteds in this service.</returns>
        protected IReadOnlyCollection<MariWebSocket> GetAllClients()
            => Sockets.Values.ToHashSet();

        /// <summary>
        /// Dispose that service instance.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            var clientsSpan = new ReadOnlySpan<MariWebSocket>(Sockets.Values.ToArray());

            foreach (var client in clientsSpan)
            {
                client.WebSocket.Abort();
                client.Dispose();
            }

            Sockets.Clear();

            if (!Cts.IsCancellationRequested)
                Cts.Cancel();
            Cts.Dispose();

            IsDisposed = true;
        }
    }
}