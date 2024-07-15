using BlazorServiceWorkerDemo.Components;
using BlazorServiceWorkerDemo.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.WebWorkers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
// SpawnDev.BlazorJS
builder.Services.AddBlazorJSRuntime(out var JS);
// SpawnDev.BlazorJS.WebWorkers
builder.Services.AddWebWorkerService();

builder.Services.AddScoped((sp) => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register a ServiceWorker handler (PWAServiceWorker here) that inherits from ServiceWorkerEventHandler
builder.Services.RegisterServiceWorker<PWAServiceWorker>(new ServiceWorkerConfig
{
    ImportServiceWorkerAssets = true,
});

if (JS.IsWindow)
{
    JS.Log("IsWindow");
}
else if (JS.IsServiceWorkerGlobalScope)
{
    JS.Log("IsServiceWorkerGlobalScope");
}

// Or Register a ServiceWorker handler (AppServiceWorker here) that inherits from ServiceWorkerEventHandler
//builder.Services.RegisterServiceWorker<AppServiceWorker>();

// Or UnregisterServiceWorker the ServiceWorker if no longer desired
//builder.Services.UnregisterServiceWorker();
// SpawnDev.BlazorJS startup (replaces RunAsync())
await builder.Build().BlazorJSRunAsync();

