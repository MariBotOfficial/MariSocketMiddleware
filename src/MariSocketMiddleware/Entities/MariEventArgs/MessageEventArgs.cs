using System;
using System.Collections.Generic;
using System.Text;

namespace MariSocketMiddleware.Entities.MariEventArgs
{
    /// <summary>
    /// The <see cref="EventArgs"/> of the message event.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        internal MessageEventArgs(MariWebSocket webSocket, string message)
        {
            WebSocket = webSocket;
            Message = message;
        }

        /// <summary>
        /// The WebSocket who sends the message.
        /// </summary>
        public MariWebSocket WebSocket { get; }

        /// <summary>
        /// The message received.
        /// </summary>
        public string Message { get; set; }
    }
}