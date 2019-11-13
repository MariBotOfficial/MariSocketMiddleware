using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace MariSocketMiddleware
{
    public readonly struct MariWebSocket
    {
        public readonly string Id;
        public readonly WebSocket WebSocket;

        public MariWebSocket(WebSocket webSocket)
        {
            Id = Guid.NewGuid().ToString();
            WebSocket = webSocket;
        }
    }
}