using Microsoft.AspNetCore.Http;
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
        internal OpenEventArgs(MariWebSocket socket, HttpContext context)
        {
            WebSocket = socket;
            Context = context;
        }

        /// <summary>
        /// The WebSocketClient who connected in this server.
        /// </summary>
        public MariWebSocket WebSocket { get; }

        /// <summary>
        /// The Request Context.
        /// </summary>
        public HttpContext Context { get; set; }
    }
}