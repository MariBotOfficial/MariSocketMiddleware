using MariGlobals.Class.Utils;
using MariSocketMiddleware.Entities;
using MariSocketMiddleware.Middleware;
using MariSocketMiddleware.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MariSocketMiddleware.Utils
{
    /// <summary>
    /// Extensions for the MariWebSocketMiddleware package.
    /// </summary>
    public static class MariWebSocketMiddlewareExtensions
    {
        /// <summary>
        /// Enable WebSockets requests and add the <see cref="MariWebSocketMiddleware"/> to your app.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseMariWebSockets(this IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.UseMiddleware<MariWebSocketMiddleware>();
            return app;
        }

        /// <summary>
        /// Enable WebSockets requests and add the <see cref="MariWebSocketMiddleware"/> to your app.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="options">The base <see cref="WebSocketOptions"/>.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseMariWebSockets(this IApplicationBuilder app, WebSocketOptions options)
        {
            app.UseWebSockets(options);
            app.UseMiddleware<MariWebSocketMiddleware>();
            return app;
        }

        /// <summary>
        /// Enable WebSockets requests and add the <see cref="MariWebSocketMiddleware"/> to your app.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="configure">An <see cref="Action"/>to configure the base <see cref="WebSocketOptions"/>.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseMariWebSockets(this IApplicationBuilder app, Action<WebSocketOptions> configure)
        {
            var options = new WebSocketOptions();
            configure(options);
            return app.UseMariWebSockets(options);
        }

        /// <summary>
        /// Add a MariWebSocketService.
        /// </summary>
        /// <typeparam name="T">A type that inherits from <see cref="MariBaseWebSocketService"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="serviceIstance">An instance of your inherited <see cref="MariBaseWebSocketService"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddMariWebSocketService<T>(this IServiceCollection services, T serviceIstance)
            where T : MariBaseWebSocketService
        {
            services.AddSingleton<IMariWebSocketService>(serviceIstance);
            return services;
        }

        /// <summary>
        /// Add a MariWebSocketService.
        /// </summary>
        /// <typeparam name="T">A type that inherits from <see cref="MariBaseWebSocketService"/></typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddMariWebSocketService<T>(this IServiceCollection services)
            where T : MariBaseWebSocketService
        {
            services.AddSingleton<IMariWebSocketService, T>();
            return services;
        }

        /// <summary>
        /// Try await the task and calls
        /// <see cref="MariBaseWebSocketService"/>#OnErroAsync
        /// if a exception throws.
        /// </summary>
        /// <typeparam name="T">The <see cref="ILogger"/> type.</typeparam>
        /// <param name="task">The <see cref="Task"/> that will await.</param>
        /// <param name="logger">The ASP.NET Core <see cref="ILogger"/> for the Middleware.</param>
        /// <param name="service">The Socketservice.</param>
        /// <param name="socket">The <see cref="MariWebSocket"/>.</param>
        /// <param name="cancel">A Boolen indicates if that Method will throw the exception or not.</param>
        /// <returns></returns>
        internal static async Task Try<T>
            (this Task task, ILogger<T> logger, MariBaseWebSocketService service, MariWebSocket socket, bool cancel = true)
        {
            await task
                .TryAsync((ex) => HandleExceptionFromTryAsync(ex, logger, service, socket, cancel));
        }

        /// <summary>
        /// Try await the Task and calls
        /// <see cref="MariBaseWebSocketService"/>#OnErroAsync
        /// if a exception throws.
        /// </summary>
        /// <typeparam name="T">The <see cref="ILogger"/> type.</typeparam>
        /// <typeparam name="TResult">The Result of the awaitable Task.</typeparam>
        /// <param name="task">The <see cref="Task"/> that will await.</param>
        /// <param name="logger">The ASP.NET Core <see cref="ILogger"/> for the Middleware.</param>
        /// <param name="service">The Socketservice.</param>
        /// <param name="socket">The <see cref="MariWebSocket"/>.</param>
        /// <param name="cancel">A Boolen indicates if that Method will throw the exception or not.</param>
        /// <returns>A <see cref="Task"/> with the result (will return default if a exception is catched).
        /// </returns>
        internal static async Task<TResult> Try<T, TResult>
            (this Task<TResult> task, ILogger<T> logger, MariBaseWebSocketService service, MariWebSocket socket, bool cancel = true)
        {
            return await task
                .TryAsync((ex) => HandleExceptionFromTryAsync(ex, logger, service, socket, cancel));
        }

        private static async Task HandleExceptionFromTryAsync<T>
            (Exception ex, ILogger<T> logger, MariBaseWebSocketService service, MariWebSocket socket, bool cancel)
        {
            await service.OnErrorAsync(ex, socket);

            if (cancel)
                throw ex;
            else if (logger.HasContent())
                logger.LogError(ex, ex.Message);
        }
    }
}