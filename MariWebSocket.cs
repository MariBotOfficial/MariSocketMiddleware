using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MariSocketMiddleware
{
    /// <summary>
    /// Represents a WebSocket client.
    /// </summary>
    public readonly struct MariWebSocket : IDisposable
    {
        /// <summary>
        /// The unique identifier for this WebSocket.
        /// </summary>
        public readonly string Id;

        internal readonly WebSocket WebSocket;

        private readonly CancellationToken _token;
        private readonly MariWebSocketService _service;

        internal MariWebSocket(WebSocket webSocket, CancellationToken token, MariWebSocketService service)
        {
            Id = Guid.NewGuid().ToString();
            WebSocket = webSocket;
            _token = token;
            _service = service;
        }

        /// <summary>
        /// Dispose that instance.
        /// </summary>
        public void Dispose()
            => WebSocket.Dispose();

        /// <summary>
        /// Send a message to this WebSocket client.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns></returns>
        public async Task SendAsync(string message)
        {
            if (!WebSocket.State.Equals(WebSocketState.Open))
                return;

            await WebSocket.SendAsync(
                Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, _token)
                .Try<MariWebSocket>(null, _service, this, false);
        }
    }
}