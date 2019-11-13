using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MariSocketMiddleware
{
    public abstract class MariWebSocketService : IDisposable
    {
        private readonly ConcurrentDictionary<string, MariWebSocket> Sockets;
        internal readonly CancellationTokenSource Cts;

        internal MariWebSocketService()
        {
            Sockets = new ConcurrentDictionary<string, MariWebSocket>();
            Cts = new CancellationTokenSource();
        }

        /// <summary>
        /// The Path of this service.
        /// </summary>
        public abstract string Path { get; set; }

        /// <summary>
        /// The method that will authorize or not the request,
        /// </summary>
        /// <param name="context">The HttpContext of the request,</param>
        /// <returns>A boolean that represents true for authorized or for unauthorized.</returns>
        public abstract Task<bool> AuthorizeAsync(HttpContext context);

        /// <summary>
        /// Indicate when a WebSocket connection is successfully opened.
        /// </summary>
        /// <returns></returns>
        public abstract Task OnOpenAsync();

        internal void AddClient(MariWebSocket socket)
            => Sockets.TryAdd(socket.Id, socket);

        internal void RemoveClient(MariWebSocket socket)
            => RemoveClient(socket.Id);

        internal void RemoveClient(string id)
            => Sockets.TryRemove(id, out var _);

        /// <summary>
        /// Get All Clients connecteds in this service.
        /// </summary>
        /// <returns>An IReadOnlyCollection with all clients connecteds in this service.</returns>
        protected IReadOnlyCollection<MariWebSocket> GetAllClients()
            => Sockets.Values.ToHashSet();

        public void Dispose()
        {
            Cts.Cancel();
        }
    }
}