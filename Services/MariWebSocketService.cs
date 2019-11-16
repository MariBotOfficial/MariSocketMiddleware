using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using MariGlobals.Class.Event;
using MariSocketMiddleware.Entities.MariEventArgs;
using MariSocketMiddleware.Entities;
using MariGlobals.Class.Utils;
using ErrorEventArgs = MariSocketMiddleware.Entities.MariEventArgs.ErrorEventArgs;

namespace MariSocketMiddleware.Services
{
    /// <summary>
    /// The base WebSocketService event-based.
    /// </summary>
    public abstract class MariWebSocketService : MariBaseWebSocketService
    {
        /// <summary>
        /// Creates a instance of <see cref="MariWebSocketService"/>
        /// </summary>
        public MariWebSocketService()
        {
            _onOpen = new AsyncEvent<OpenEventArgs>();
            _onError = new AsyncEvent<ErrorEventArgs>();
            _onDisconnected = new AsyncEvent<DisconnectEventArgs>();
            _onMessage = new AsyncEvent<MessageEventArgs>();
        }

        /// <summary>
        /// Fired when a Connection is opened.
        /// </summary>
        protected event AsyncEventHandler<OpenEventArgs> OnOpen
        {
            add => _onOpen.Register(value);
            remove => _onOpen.Unregister(value);
        }

        private readonly AsyncEvent<OpenEventArgs> _onOpen;

        /// <summary>
        /// Fired when a Error is generated.
        /// </summary>
        protected event AsyncEventHandler<ErrorEventArgs> OnError
        {
            add => _onError.Register(value);
            remove => _onError.Unregister(value);
        }

        private readonly AsyncEvent<ErrorEventArgs> _onError;

        /// <summary>
        /// Fired when a WebSocketClient is disconnected.
        /// </summary>
        protected event AsyncEventHandler<DisconnectEventArgs> OnDisconnected
        {
            add => _onDisconnected.Register(value);
            remove => _onDisconnected.Unregister(value);
        }

        private readonly AsyncEvent<DisconnectEventArgs> _onDisconnected;

        /// <summary>
        /// Fired when a WebSocketClient sends a message for this server.
        /// </summary>
        protected event AsyncEventHandler<MessageEventArgs> OnMessage
        {
            add => _onMessage.Register(value);
            remove => _onMessage.Unregister(value);
        }

        private readonly AsyncEvent<MessageEventArgs> _onMessage;

        internal override Task OnOpenAsync(MariWebSocket socket)
                => _onOpen.InvokeAsync(new OpenEventArgs(socket));

        internal override Task OnErrorAsync(Exception exception, MariWebSocket socket)
                => _onError.InvokeAsync(new ErrorEventArgs(exception, socket));

        internal override Task OnDisconnectedAsync
            (MariWebSocket socket, WebSocketCloseStatus code, string reason)
                => _onDisconnected.InvokeAsync(new DisconnectEventArgs(socket, code, reason));

        internal override Task OnMessageAsync(MariWebSocket socket, string message)
                => _onMessage.InvokeAsync(new MessageEventArgs(socket, message));
    }
}