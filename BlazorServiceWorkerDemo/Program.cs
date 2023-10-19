using BlazorServiceWorkerDemo.Components;
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
            // Our ServiceWorker handler AppServiceWorker (inherits from ServiceWorkerManager)
            builder.Services.RegisterServiceWorker<AppServiceWorker>();
            // SpawnDev.BlazorJS startup (replaces RunAsync())
            await builder.Build().BlazorJSRunAsync();
        }
    }
}
