using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;

namespace MariSocketMiddleware
{

	///<inheritdoc/>
    public abstract class MariWebSocketEventService : MariWebSocketService
    {
    	protected Task delegate OpenHandler<MariWebSocket>();
    	protected Task delegate ErrorHandler<Exception, MariWebSocket>();
    	protected Task delegate DisconnectedHandler<MariWebSocket, WebSocketCloseStatus, string>();
    	protected Task delegate MessageHandler<MariWebSocket, string>();
    	
    	protected event OpenHandler OnOpen;
    	protected event ErrorHandler OnError;
    	protected event DisconnectedHandler OnDisconnected;
    	protected event MessageHandler OnMessage;
    	
    	///<inheritdoc/>
    	public abstract Task<bool> AuthotizeAsync(HttpContext context);
    	
    	///<inheritdoc/>
    	public abstract string Path { get; set; }
    	
    	///<inheritdoc/>
    	public override async Task OnOpenAsync(MariWebSocket socket)
    	{
    		if(OnOpen.HasContent())
    			await OnOpen(socket);
    	}
    	
    	///<inheritdoc/>
    	public override async Task OnErrorAsync(Exception exception, MariWebSocket socket)
    	{
    		if(OnError.HasContent())
    			await OnError(exception, socket);
    	}

		///<inheritdoc/>
		public override async Task OnDisconnectedAsync
            (MariWebSocket socket, WebSocketCloseStatus code, string reason)
        {
            	if(OnDisconnected.HasContent())
            		await OnDisconnected(socket, code, reason);
        }

		///<inheritdoc/>
		public override async Task OnMessageAsync(MariWebSocket socket, string message)
		{
			if(OnMessage.HasContent())
				await OnMessage(socket, message);
		}
    }
}