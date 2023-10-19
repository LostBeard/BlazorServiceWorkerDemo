using BlazorServiceWorkerDemo.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.WebWorkers;

namespace BlazorServiceWorkerDemo
{
    public class Apples
    {
        public Apples()
        {
            Console.WriteLine("Apples()");
        }
    }
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            // shared services
            builder.Services.AddBlazorJSRuntime();
            var JS = BlazorJSRuntime.JS;
            if (JS.IsWindow) await WindowContext(builder, JS);
            else if (JS.IsServiceWorkerGlobalScope) await ServiceWorkerContext(builder, JS);
            else if (JS.IsDedicatedWorkerGlobalScope) await WebWorkerContext(builder, JS);
            else if (JS.IsSharedWorkerGlobalScope) await WebWorkerContext(builder, JS);
        }
        static async Task WebWorkerContext(WebAssemblyHostBuilder builder, BlazorJSRuntime JS)
        {
            Console.WriteLine("WebWorkerContext");
            builder.Services.AddWebWorkerService();
            builder.Services.RegisterServiceWorker<ServiceWorkerManager>();
            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            await builder.Build().BlazorJSRunAsync();
        }
        static async Task ServiceWorkerContext(WebAssemblyHostBuilder builder, BlazorJSRuntime JS)
        {
            Console.WriteLine("ServiceWorkerContext");
            builder.Services.AddWebWorkerService();
            builder.Services.RegisterServiceWorker<ServiceWorkerManager>();
            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            await builder.Build().BlazorJSRunAsync();
        }
        static async Task WindowContext(WebAssemblyHostBuilder builder, BlazorJSRuntime JS)
        {
            Console.WriteLine("WindowContext");
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            builder.Services.AddWebWorkerService();
            builder.Services.RegisterServiceWorker<ServiceWorkerManager>();
            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            await builder.Build().BlazorJSRunAsync();
        }
    }
}
