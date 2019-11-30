using System;
using System.Collections.Generic;
using System.Text;

namespace MariSocketMiddleware.Entities.MariEventArgs
{
    /// <summary>
    /// <see cref="EventArgs"/> for the open event.
    /// </summary>
    public class OpenEventArgs : EventArgs
    {
        internal OpenEventArgs(MariWebSocket socket)
        {
            WebSocket = socket;
        }

        /// <summary>
        /// The WebSocketClient who connected in this server.
        /// </summary>
        public MariWebSocket WebSocket { get; }
    }
}