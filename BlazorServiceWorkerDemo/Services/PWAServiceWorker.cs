using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.WebWorkers;

namespace BlazorServiceWorkerDemo.Services
{
    public class PWAServiceWorker : ServiceWorkerEventHandler
    {
        ServiceWorkerGlobalScope? self = null;
        AssetManifest? assetsManifest = null;
        Uri baseUri;
        string cacheNamePrefix = "offline-cache-";
        string cacheName = "";
        List<string>? manifestUrlList = null;
        CacheStorage? caches = null;
        List<string> offlineAssetsInclude = [".dll", ".pdb", ".wasm", ".html", ".js", ".json", ".css", ".woff", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".blat", ".dat"];
        List<string> offlineAssetsExclude = ["service-worker.js"];
        bool isProduction;
        public PWAServiceWorker(BlazorJSRuntime js, NavigationManager navigationManager, IWebAssemblyHostEnvironment hostEnvironment) : base(js)
        {
            baseUri = new Uri(navigationManager.BaseUri);
            isProduction = hostEnvironment.IsProduction();
        }

        // called before any ServiceWorker events are handled
        protected override async Task OnInitializedAsync()
        {
            // By default, this service is only started in a ServiceWorker but it may start in other scopes if injected into another service.
            // You can do initialization based on the scope that is running.
            Log("GlobalThisTypeName:", JS.GlobalThisTypeName, "Production:", isProduction);
            self = JS.ServiceWorkerThis;
            if (self != null)
            {
                // get the assets manifest data generated on release build and imported in the service-worker.js
                assetsManifest = JS.Get<AssetManifest?>("assetsManifest");
                if (assetsManifest != null)
                {
                    cacheName = $"{cacheNamePrefix}{assetsManifest.Version}";
                    manifestUrlList = assetsManifest!.Assets.Select(asset => new Uri(baseUri, asset.Url).ToString()).ToList();
                }
                caches = self.Caches;
            }
        }

        protected override async Task ServiceWorker_OnInstallAsync(ExtendableEvent e)
        {
            Log($"ServiceWorker_OnInstallAsync");

            if (!isProduction || assetsManifest == null)
            {
                return;
            }

            // Fetch and cache all matching items from the assets manifest
            var assetsRequests = assetsManifest!.Assets
                .Where(asset => offlineAssetsInclude.Any(o => asset.Url.EndsWith(o)))
                .Where(asset => !offlineAssetsExclude.Any(o => asset.Url.Equals(o)))
                .Select(asset => new Request(asset.Url, new RequestOptions { Integrity = asset.Hash, Cache = "no-cache" }))
                .ToList();

            var cache = await caches!.Open(cacheName);
            await cache.AddAll(assetsRequests);
        }

        protected override async Task ServiceWorker_OnActivateAsync(ExtendableEvent e)
        {
            Log($"ServiceWorker_OnActivateAsync");

            if (!isProduction || assetsManifest == null)
            {
                return;
            }

            // Delete unused caches
            var cacheKeys = await caches!.Keys();
            await Task.WhenAll(cacheKeys
                .Where(key => key.StartsWith(cacheNamePrefix) && key != cacheName)
                .Select(key => caches.Delete(key)));
        }

        protected override async Task<Response> ServiceWorker_OnFetchAsync(FetchEvent e)
        {
            Log($"ServiceWorker_OnFetchAsync", e.Request.Method, e.Request.Url);
            Response? response = null;
            if (e.Request.Method == "GET")
            {
                // For all navigation requests, try to serve index.html from cache,
                // unless that request is for an offline resource.
                // If you need some URLs to be server-rendered, edit the following check to exclude those URLs
                var shouldServeIndexHtml = e.Request.Mode == "navigate" && !manifestUrlList!.Any(url => url == e.Request.Url);
                var request = shouldServeIndexHtml ? new Request("index.html") : e.Request;
                var cache = await caches!.Open(cacheName);
                response = await cache.Match(request);
            }
            if (response == null)
            {
                try
                {
                    response = await JS.Fetch(e.Request);
                }
                catch (Exception ex)
                {
                    response = new Response(ex.Message, new ResponseOptions { Status = 500, StatusText = ex.Message, Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } } });
                }
            }
            return response;
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

        void Log(params object[] args)
        {
            JS.Log(new object?[] { $"ServiceWorkerEventHandler: {InstanceId}" }.Concat(args).ToArray());
        }
    }
}
