using System;
using System.Collections.Generic;
using System.Text;

namespace MariSocketMiddleware.Entities.MariEventArgs
{
    /// <summary>
    /// The <see cref="EventArgs"/> for the error event.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The exception generated.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The <see cref="MariWebSocket"/> who generates the exception (can be null).
        /// </summary>
        public MariWebSocket WebSocket { get; }

        internal ErrorEventArgs(Exception exception, MariWebSocket socket)
        {
            Exception = exception;
            WebSocket = socket;
        }
    }
}