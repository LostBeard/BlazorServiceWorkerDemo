using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.WebWorkers;
using System.ComponentModel.DataAnnotations;

namespace BlazorServiceWorkerDemo.Services
{
    public class PushNotificationSubscribeRequest
    {
        [Required]
        public string EndPoint { get; set; }

        public double? ExpirationTime { get; set; }
        public string P256dh { get; set; }
        public string Auth { get; set; }

        [Required]
        public string PublicKey { get; set; }
    }
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
                    Log("Offline cache name:", cacheName);
                    manifestUrlList = assetsManifest!.Assets.Select(asset => new Uri(baseUri, asset.Url).ToString()).ToList();
                }
                caches = self.Caches;
            }
        }

        protected override async Task ServiceWorker_OnInstallAsync(ExtendableEvent e)
        {
            Log($"ServiceWorker_OnInstallAsync");

            // cache assets (if needed)
            if (isProduction && assetsManifest != null)
            {
                try
                {
                    // Fetch and cache all matching items from the assets manifest
                    var assetsRequests = assetsManifest!.Assets
                        .Where(asset => offlineAssetsInclude.Any(o => asset.Url.EndsWith(o)))
                        .Where(asset => !offlineAssetsExclude.Any(o => asset.Url.Equals(o)))
                        .Select(asset => new Request(asset.Url, new RequestOptions { Integrity = asset.Hash, Cache = "no-cache" }))
                        .ToList();

                    var cache = await caches!.Open(cacheName);
                    await cache.AddAll(assetsRequests);
                    Log("Cached:", cacheName);
                }
                catch (Exception ex)
                {
                    Log("Failed to cache:", cacheName);
                }
            }
            // optionally skip waiting
            Log($"self.SkipWaiting()");
            await self!.SkipWaiting();
        }

        protected override async Task ServiceWorker_OnActivateAsync(ExtendableEvent e)
        {
            Log($"ServiceWorker_OnActivateAsync");

            // delete old caches
            if (isProduction)
            {
                // Delete unused caches that start with offline prefix
                var cacheKeys = await caches!.Keys();
                await Task.WhenAll(cacheKeys
                    .Where(key => key.StartsWith(cacheNamePrefix) && key != cacheName)
                    .Select(key => caches.Delete(key)));
            }
            // optionally claim all clients
            Log($"clients.Claim()");
            using var clients = self!.Clients;
            await clients.Claim();
        }

        protected override async Task<Response> ServiceWorker_OnFetchAsync(FetchEvent e)
        {
            Log($"ServiceWorker_OnFetchAsync", e.Request.Method, e.Request.Url);
            Response? response = null;
            if (e.Request.Method == "GET" && assetsManifest != null)
            {
                // For all navigation requests, try to serve index.html from cache,
                // unless that request is for an offline resource.
                // If you need some URLs to be server-rendered, edit the following check to exclude those URLs
                var shouldServeIndexHtml = e.Request.Mode == "navigate" && !manifestUrlList!.Any(url => url == e.Request.Url);
                var request = shouldServeIndexHtml ? new Request("index.html") : e.Request;
                var cache = await caches!.Open(cacheName);
                response = await cache.Match(request);
                if (response != null)
                {
                    Log("Cached response used:", cacheName, e.Request.Method, e.Request.Url);
                }
            }
            if (response == null)
            {
                try
                {
                    response = await JS.Fetch(e.Request);
                    Log("Live response used:", e.Request.Method, e.Request.Url);
                }
                catch (Exception ex)
                {
                    Log("Failed response used:", e.Request.Method, e.Request.Url);
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

        protected override async Task ServiceWorker_OnPushSubscriptionChangeAsync(PushSubscriptionChangeEvent e)
        {
            Log($"ServiceWorker_OnPushSubscriptionChangeAsync");
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
            JS.Log(new object?[] { $"ServiceWorkerEventHandler: {JS.InstanceId}" }.Concat(args).ToArray());
        }

        static string UrlSafeBase64Encode(byte[] toEncodeAsBytes)
        {
            return System.Convert.ToBase64String(toEncodeAsBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
        public async Task<PushNotificationSubscribeRequest?> Subscribe(string applicationServerKey)
        {
            PushNotificationSubscribeRequest? ret = null;
            try
            {
                //
                using var ServiceWorkerContainer = JS.Get<ServiceWorkerContainer>("navigator.serviceWorker");
                using var controller = ServiceWorkerContainer.Controller;
                var swIsActive = controller != null && controller.State == "activated";
                if (!swIsActive)
                {
                    // unsupported
                    return null;
                }
                var serviceWorkerRegistration = await ServiceWorkerContainer.Ready;
                if (serviceWorkerRegistration == null)
                {
                    // unsupported
                    return null;
                }
                // check permissions (required on android, skippable on windows desktop)
                var permGranted = await NotificationPermissionGranted(true);
                if (!permGranted)
                {
                    // permission denied
                    Console.WriteLine("Notification subscription cancelled. Permission denied.");
                    return null;
                }
                //
                using var pushManager = serviceWorkerRegistration.PushManager;
                if (pushManager == null)
                {
                    // unsupported
                    return null;
                }
                // unsub in case already subbed
                using var subscriptionOld = await pushManager.GetSubscription();
                if (subscriptionOld != null)
                {
                    // already has a subscription
                    // unsub so we can resub
                    await subscriptionOld.Unsubscribe();
                }
                //
                var subOptions = new PushManagerSubscribeOptions
                {
                    ApplicationServerKey = applicationServerKey,
                    UserVisibleOnly = true,
                };
                using var subscription = await pushManager.Subscribe(subOptions);
                if (subscription == null)
                {
                    return null;
                }
                // get subscription auth key
                using var authKey = subscription.GetKey("auth");
                var authKeyBytes = authKey.ReadBytes();
                var authKeyUrlSafeBase64 = UrlSafeBase64Encode(authKeyBytes);
                // get subscription p256dh key
                using var p256dhKey = subscription.GetKey("p256dh");
                var p256dhKeyBytes = p256dhKey.ReadBytes();
                var p256dhKeyUrlSafeBase64 = UrlSafeBase64Encode(p256dhKeyBytes);
                // send subscription result to server for saving
                ret = new PushNotificationSubscribeRequest
                {
                    EndPoint = subscription.Endpoint,
                    ExpirationTime = subscription.ExpirationTime,
                    PublicKey = applicationServerKey,
                    Auth = authKeyUrlSafeBase64,
                    P256dh = p256dhKeyUrlSafeBase64,
                };
            }
            catch (Exception ex)
            {
                var nmt = true;
            }
            return ret;
        }
        public bool PermissionGranted => Notification.Permission == "granted";
        public bool PermissionDenied => Notification.Permission == "denied";
        public bool PermissionMustAsk => Notification.Permission != "denied" && Notification.Permission != "granted";

        async Task<bool> NotificationPermissionGranted(bool allowAsk = false)
        {
            //if (!IsSupported) return false;
            var perm = Notification.Permission;
            Console.WriteLine($"Notification.Permission: {perm}");
            if (perm == "granted") return true;
            if (perm != "denied" && allowAsk)
            {
                Console.WriteLine($"Notification.Permission asking...");
                perm = await Notification.RequestPermission();
                // if perm still does not equal denied or granted (should be default) then the app itself has been blocked from showing notifications in android settings
            }
            Console.WriteLine($"Notification.Permission: final {perm}");
            return perm == "granted";
        }
    }
}
