# Blazor WASM ServiceWorker Demo

Blazor WASM running as your ServiceWorker! Thanks to [SpawnDev.BlazorJS.WebWorkers](https://github.com/LostBeard/SpawnDev.BlazorJS#spawndevblazorjswebworkers) it is now possible to run Blazor WASM in all browser Worker contexts: DedicatedWorker, SharedWorker, and ServiceWorker.

[Live Demo](https://lostbeard.github.io/BlazorServiceWorkerDemo/)

The live demo is nothing special at the moment. In Chrome you acn see the Blazor messages from the worker handler handling events. Firefox ServiceWorker console can be found at "about:debugging#/runtime/this-firefox"

This code demonstrates loading a Blazor WASM inside a ServiceWorker context and handling any events a ServiceWorker may want to such as fetch.

This is currently a working proof of concept and likely to change. Any and all feedback is welcome!

This project relies on my other repo [SpawnDev.BlazorJS](https://github.com/LostBeard/SpawnDev.BlazorJS) and more specifically [SpawnDev.BlazorJS.WebWorkers](https://github.com/LostBeard/SpawnDev.BlazorJS#spawndevblazorjswebworkers)

## Quick start

Add Nuget:  
SpawnDev.BlazorJS.WebWorkers

Create or modify you wwwroot/service-worker.js in your Blazor WASM project. 
```js
importScripts('_content/SpawnDev.BlazorJS.WebWorkers/spawndev.blazorjs.webworkers.js');
```

Example Program.cs  
```cs
var builder = WebAssemblyHostBuilder.CreateDefault(args);
// SpawnDev.BlazorJS
builder.Services.AddBlazorJSRuntime();
// SpawnDev.BlazorJS.WebWorkers
builder.Services.AddWebWorkerService();
// Our ServiceWorker handler AppServiceWorker (inherits from ServiceWorkerManager)
builder.Services.RegisterServiceWorker<AppServiceWorker>();
builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// SpawnDev.BlazorJS startup (replaces RunAsync())
await builder.Build().BlazorJSRunAsync();
```

Example AppServiceWorker.cs  
```cs
public class AppServiceWorker : ServiceWorkerManager
{
    public AppServiceWorker(BlazorJSRuntime js) : base(js)
    {

    }

    // called before any ServiceWorker events are handled
    protected override async Task OnInitialized()
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

    protected override void OnPushSubscriptionChange(Event e)
    {
        Log($"OnPushSubscriptionChange");
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