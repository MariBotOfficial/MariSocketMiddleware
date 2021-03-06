﻿using MariSocketMiddleware.Services;
using MariSocketMiddleware.Utils;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MariSocketMiddleware.Entities
{
    /// <summary>
    /// Represents a WebSocket client.
    /// </summary>
    public readonly struct MariWebSocket : IDisposable
    {
        /// <summary>
        /// The unique identifier for this WebSocket.
        /// </summary>
        public string Id { get; }

        internal readonly WebSocket WebSocket;

        private readonly CancellationToken _token;
        private readonly MariBaseWebSocketService _service;

        internal MariWebSocket(WebSocket webSocket, CancellationToken token, MariBaseWebSocketService service)
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
        /// <param name="obj">The objeto to be serialized.</param>
        /// <returns></returns>
        public Task SendAsync(object obj)
            => SendAsync(JsonSerializer.Serialize(obj));

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

        /// <summary>
        /// Close the connection with the Websocket client.
        /// </summary>
        /// <param name="status">The <see cref="WebSocketCloseStatus"/>.</param>
        /// <param name="message">The close's reason.</param>
        /// <returns></returns>
        public async Task CloseAsync(WebSocketCloseStatus status, string message)
        {
            if (!WebSocket.State.Equals(WebSocketState.Open))
                return;

            await WebSocket.CloseAsync(status, message, _token)
                .Try<MariWebSocket>(null, _service, this, false);
        }
    }
}