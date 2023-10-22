using BlazorServiceWorkerDemo.Components;
using BlazorServiceWorkerDemo.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.WebWorkers;

namespace BlazorServiceWorkerDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            // SpawnDev.BlazorJS
            builder.Services.AddBlazorJSRuntime();
            // SpawnDev.BlazorJS.WebWorkers
            builder.Services.AddWebWorkerService();

            // Register a ServiceWorker handler (PWAServiceWorker here) that inherits from ServiceWorkerEventHandler
            builder.Services.RegisterServiceWorker<PWAServiceWorker>();

            // Or Register a ServiceWorker handler (AppServiceWorker here) that inherits from ServiceWorkerEventHandler
            //builder.Services.RegisterServiceWorker<AppServiceWorker>();

            // Or UnregisterServiceWorker the ServiceWorker if no longer desired
            //builder.Services.UnregisterServiceWorker();
            // SpawnDev.BlazorJS startup (replaces RunAsync())
            await builder.Build().BlazorJSRunAsync();
        }
    }
}
