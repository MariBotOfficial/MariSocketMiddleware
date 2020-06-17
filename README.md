# MariSocketMiddleware

An ASP.NET Core Middleware Event-based for multiple WebSockets services with easy and low memory usage.

# Usage

**MariSocketMiddleware** is very easy to use, just implement that in your ASP.NET core project.

## Service class

```csharp
using MariSocketMiddleware.Entities.MariEventArgs;
using MariSocketMiddleware.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MyApi
{
    public class MyWebSocketService : MariWebSocketService
    {
        public MyWebSocketService()
        {
            OnOpen += OpenAsync;
            OnDisconnected += OnDisconnectedAsync;
            OnError += OnErrorAsync;
            OnMessage += OnMessageAsync;
        }

        private Task OnMessageAsync(MessageEventArgs arg)
            => Task.CompletedTask;

        private Task OnErrorAsync(ErrorEventArgs arg)
            => Task.CompletedTask;

        private Task OnDisconnectedAsync(DisconnectEventArgs arg)
            => Task.CompletedTask;

        private Task OpenAsync(OpenEventArgs arg)
            => Task.CompletedTask;

        public override string Path { get; set; } = "/Gateway";

        public override Task<bool> AuthorizeAsync(HttpContext context)
            => Task.FromResult(true);
    }
}
```

## In Startup.cs

```csharp
using MariSocketMiddleware.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Here
            // The generic type or the instance.
            services.AddMariWebSocketService<MyWebSocketService>();

            // ASP.NET Core template.
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // ASP.NET Core template.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Here
            app.UseMariWebSockets();

            // ASP.NET Core template.
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
```

## Additional

Your service will be injected in the ASP.NET Core D.I, therefore you can get your service instance or get others services in your service.

### Getting other dependencie

```csharp
using MariSocketMiddleware.Entities.MariEventArgs;
using MariSocketMiddleware.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MyApi
{
    public class MyWebSocketService : MariWebSocketService
    {
        private readonly ILogger<MyWebSocketService> _logger;

        // ASP.NET Core will pass the dependencies in your class ctor.
        public MyWebSocketService(ILogger<MyWebSocketService> logger)
        {
            _logger = logger;
            OnOpen += OpenAsync;
            OnDisconnected += OnDisconnectedAsync;
            OnError += OnErrorAsync;
            OnMessage += OnMessageAsync;
        }

        private Task OnMessageAsync(MessageEventArgs arg)
            => Task.CompletedTask;

        private Task OnErrorAsync(ErrorEventArgs arg)
            => Task.CompletedTask;

        private Task OnDisconnectedAsync(DisconnectEventArgs arg)
            => Task.CompletedTask;

        private Task OpenAsync(OpenEventArgs arg)
            => Task.CompletedTask;

        public override string Path { get; set; } = "/Gateway";

        public override Task<bool> AuthorizeAsync(HttpContext context)
            => Task.FromResult(true);
    }
}
```

### Getting your service

You can just pass your service in other service ctor or do a IServiceProvider#GetService.

```csharp
using System;
using Microsoft.AspNetCore.Mvc;

namespace MyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly MyWebSocketService _service1;
        private readonly MyOtherWebSocketService _service2;

        public ValuesController(MyWebSocketService service1, IServiceProvider provider)
        {
            // Direct requesting your service or
            _service1 = service1;

            // Requesting for your provider.
            _service2 = provider.GetService<MyOtherWebSocketService>();
        }

        // GET api/values
        [HttpGet]
        public ActionResult Get()
            => Ok(new string[] { "value1", "value2" });
    }
}
```

# License

**MariSocketMiddleware** is provided under [The MIT License.](https://github.com/MariBotOfficial/MariSocketMiddleware/blob/master/LICENSE)
