using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace MariSocketMiddleware.Entities.MariEventArgs
{
    /// <summary>
    /// The <see cref="EventArgs"/> for the disconnect event.
    /// </summary>
    public class DisconnectEventArgs : EventArgs
    {
        internal DisconnectEventArgs(MariWebSocket webSocket, WebSocketCloseStatus code, string reason)
        {
            WebSocket = webSocket;
            Code = code;
            Reason = reason;
        }

        /// <summary>
        /// The WebSocketClient disconnected.
        /// </summary>
        public MariWebSocket WebSocket { get; }

        /// <summary>
        /// The WebSocket Status Code of the closed connection.
        /// </summary>
        public WebSocketCloseStatus Code { get; }

        /// <summary>
        /// The text reason of why the connection is closed (can be null).
        /// </summary>
        public string Reason { get; }
    }
}