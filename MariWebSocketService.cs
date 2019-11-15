using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;

namespace MariSocketMiddleware
{
    /// <summary>
    /// The base WebSocketService event-based.
    /// </summary>
    public abstract class MariWebSocketService : MariBaseWebSocketService
    {
        /// <summary>
        /// Represents a <see cref="Delegate"/> Handler for the Open event.
        /// </summary>
        /// <param name="socket">The <see cref="MariWebSocket"/> who connected in this server.</param>
        /// <returns></returns>
    	protected delegate Task OpenHandler(MariWebSocket socket);

        /// <summary>
        /// Represents a <see cref="Delegate"/> Handler for the Error event.
        /// </summary>
        /// <param name="exception">The error.</param>
        /// <param name="socket">The <see cref="MariWebSocket"/> where the <see cref="Exception"/> is generated (can be null).</param>
        /// <returns></returns>
    	protected delegate Task ErrorHandler(Exception exception, MariWebSocket socket);

        /// <summary>
        /// Represents a <see cref="Delegate"/> Handler for the Disconnected event.
        /// </summary>
        /// <param name="socket">The <see cref="MariWebSocket"/> who disconnected in this server.</param>
        /// <param name="code">The <see cref="WebSocketCloseStatus"/>.</param>
        /// <param name="reason">The disconnect reason.</param>
        /// <returns></returns>
    	protected delegate Task DisconnectedHandler(MariWebSocket socket, WebSocketCloseStatus code, string reason);

        /// <summary>
        /// Represents a <see cref="Delegate"/> Handler for the Message event.
        /// </summary>
        /// <param name="socket">The <see cref="MariWebSocket"/> who sends the message.</param>
        /// <param name="message">The message received.</param>
        /// <returns></returns>
    	protected delegate Task MessageHandler(MariWebSocket socket, string message);

        /// <summary>
        /// Fired when a Connection is opened.
        /// </summary>
        protected event OpenHandler OnOpen;

        /// <summary>
        /// Fired when a Error is generated.
        /// </summary>
        protected event ErrorHandler OnError;

        /// <summary>
        /// Fired when a WebSocketClient is disconnected.
        /// </summary>
        protected event DisconnectedHandler OnDisconnected;

        /// <summary>
        /// Fired when a WebSocketClient sends a message for this server.
        /// </summary>
        protected event MessageHandler OnMessage;

        internal override async Task OnOpenAsync(MariWebSocket socket)
        {
            if (OnOpen.HasContent())
                await OnOpen(socket);
        }

        internal override async Task OnErrorAsync(Exception exception, MariWebSocket socket)
        {
            if (OnError.HasContent())
                await OnError(exception, socket);
        }

        internal override async Task OnDisconnectedAsync
            (MariWebSocket socket, WebSocketCloseStatus code, string reason)
        {
            if (OnDisconnected.HasContent())
                await OnDisconnected(socket, code, reason);
        }

        internal override async Task OnMessageAsync(MariWebSocket socket, string message)
        {
            if (OnMessage.HasContent())
                await OnMessage(socket, message);
        }
    }
}