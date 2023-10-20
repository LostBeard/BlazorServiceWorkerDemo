using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.WebWorkers;

namespace BlazorServiceWorkerDemo
{
    public class AppServiceWorker : ServiceWorkerEventHandler
    {
        public AppServiceWorker(BlazorJSRuntime js) : base(js)
        {

        }

        // called before any ServiceWorker events are handled
        protected override async Task OnInitializedAsync()
        {
            // By default, this service is only started in a ServiceWorker but it may start in other scopes.
            // If JS.IsWindow == true be careful not stall here.
            // You can do initialization based on the scope that is running.
            Log("GlobalThisTypeName", JS.GlobalThisTypeName);
        }

        protected override async Task ServiceWorker_OnInstallAsync(ExtendableEvent e)
        {
            Log($"ServiceWorker_OnInstallAsync");
            _ = ServiceWorkerThis!.SkipWaiting();   // returned task can be ignored
        }

        void Log(params object[] args)
        {
            JS.Log(new object?[] { $"ServiceWorkerEventHandler: {InstanceId}" }.Concat(args).ToArray());
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
}
