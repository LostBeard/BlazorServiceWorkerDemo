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
            // SpawnDev.BlazorJS
            builder.Services.AddBlazorJSRuntime();
            // SpawnDev.BlazorJS.WebWorkers
            builder.Services.AddWebWorkerService();
            // Our ServiceWorker handler AppServiceWorker (inherits from ServiceWorkerManager)
            builder.Services.RegisterServiceWorker<AppServiceWorker>();
            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            // SpawnDev.BlazorJS startup (replaces RunAsync())
            await builder.Build().BlazorJSRunAsync();
        }
    }
}
