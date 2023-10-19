# Blazor WASM ServiceWorker Demo

Blazor WASM running as your ServiceWorker! Thanks to [SpawnDev.BlazorJS.WebWorkers](https://github.com/LostBeard/SpawnDev.BlazorJS#spawndevblazorjswebworkers) it is now possible to run Blazor WASM in all browser Worker contexts: DedicatedWorker, SharedWorker, and ServiceWorker.

[Live Demo](https://lostbeard.github.io/BlazorServiceWorkerDemo/)

The live demo is nothing special at the moment. In Chrome you acn see the Blazor messages from the worker handler handling events. Firefox ServiceWorker console can be found at "about:debugging#/runtime/this-firefox"

This code demonstrates loading a Blazor WASM inside a ServiceWorker context and handling any events a ServiceWorker may want to such as fetch.

This is currently a working proof of concept and likely to change. Any and all feedback is welcome!

This project relies on my other repo [SpawnDev.BlazorJS](https://github.com/LostBeard/SpawnDev.BlazorJS) and more specifically [SpawnDev.BlazorJS.WebWorkers](https://github.com/LostBeard/SpawnDev.BlazorJS#spawndevblazorjswebworkers)

## Quick Start
A very basic verbose quick start example. Create a new .Net 8 RC2 Blazor WASM project. 

### Add Nuget
SpawnDev.BlazorJS.WebWorkers 2.2.20 or later

### wwwroot/service-worker.js
```js
importScripts('_content/SpawnDev.BlazorJS.WebWorkers/spawndev.blazorjs.webworkers.js');
```

### Program.cs
```cs
var builder = WebAssemblyHostBuilder.CreateDefault(args);
// SpawnDev.BlazorJS
builder.Services.AddBlazorJSRuntime();
// SpawnDev.BlazorJS.WebWorkers
builder.Services.AddWebWorkerService();
// Our ServiceWorker handler AppServiceWorker (inherits from ServiceWorkerManager)
builder.Services.RegisterServiceWorker<AppServiceWorker>();
// SpawnDev.BlazorJS startup (replaces RunAsync())
await builder.Build().BlazorJSRunAsync();
```

### AppServiceWorker.cs - A verbose service worker example.
- Handle ServiceWorker desired events by overriding the ServiceWorkerManager base class virtual methods.
- The ServiceWorker event handlers are only called when running in a ServiceWorkerGlobalScope context.
- The AppServiceWorker singleton will may run in any scope and therefore must be scope aware. (For example, do not try to use localStorage in a Worker scope.)
```cs
public class AppServiceWorker : ServiceWorkerManager
{
    public AppServiceWorker(BlazorJSRuntime js) : base(js)
    {

    }

    // called before any ServiceWorker events are handled
    protected override async Task OnInitializedAsync()
    {
        // This service will start in all scopes
        // you can do initialization based on the scope that is running
        Log("GlobalThisTypeName", JS.GlobalThisTypeName);
        await Register();
    }

    protected override async Task ServiceWorker_OnInstallAsync(ExtendableEvent e)
    {
        Log($"ServiceWorker_OnInstallAsync");
        _ = ServiceWorkerThis!.SkipWaiting();   // returned task can be ignored
    }

    protected override async Task ServiceWorker_OnActivateAsync(ExtendableEvent e)
    {
        Log($"ServiceWorker_OnActivateAsync");
        await ServiceWorkerThis!.Clients.Claim();
    }

    protected override async Task<Response> ServiceWorker_OnFetchAsync(FetchEvent e)
    {
        Log($"ServiceWorker_OnFetchAsync", e.Request.Method, e.Request.Url);
        Response ret;
        try
        {
            ret = await JS.Fetch(e.Request);
        }
        catch (Exception ex)
        {
            ret = new Response(ex.Message, new ResponseOptions { Status = 500, StatusText = ex.Message, Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } } });
            Log($"ServiceWorker_OnFetchAsync failed: {ex.Message}");
        }
        return ret;
    }

    protected override async Task ServiceWorker_OnMessageAsync(ExtendableMessageEvent e)
    {
        Log($"ServiceWorker_OnMessageAsync");
    }

    protected override async Task ServiceWorker_OnPushAsync(PushEvent e)
    {
        Log($"ServiceWorker_OnPushAsync");
    }

    protected override void ServiceWorker_OnPushSubscriptionChange(Event e)
    {
        Log($"ServiceWorker_OnPushSubscriptionChange");
    }

    protected override async Task ServiceWorker_OnSyncAsync(SyncEvent e)
    {
        Log($"ServiceWorker_OnSyncAsync");
    }

    protected override async Task ServiceWorker_OnNotificationCloseAsync(NotificationEvent e)
    {
        Log($"ServiceWorker_OnNotificationCloseAsync");
    }

    protected override async Task ServiceWorker_OnNotificationClickAsync(NotificationEvent e)
    {
        Log($"ServiceWorker_OnNotificationClickAsync");
    }
}

```