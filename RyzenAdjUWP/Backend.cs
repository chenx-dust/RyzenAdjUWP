using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace RyzenAdjUWP
{
    internal class Backend
    {
        private AppServiceConnection _appServiceConnection;
        private BackgroundTaskDeferral _appServiceDeferral;

        public event EventHandler<ValueSet> MessageReceivedEvent;

        private static Backend _instance;
        public static Backend Instance => _instance ?? (_instance = new Backend());

        private Backend() {}

        public static async Task LaunchBackend()
        {
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
        }

        public void OnBackgroundActivated(IBackgroundTaskInstance taskInstance)
        {
            AppServiceTriggerDetails appService = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            _appServiceDeferral = taskInstance.GetDeferral();
            _appServiceConnection = appService.AppServiceConnection;
            _appServiceConnection.RequestReceived += OnAppServiceRequestReceived;
            _appServiceConnection.ServiceClosed += AppServiceConnection_ServiceClosed;
        }

        private void OnAppServiceRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            AppServiceDeferral messageDeferral = args.GetDeferral();
            MessageReceivedEvent?.Invoke(this, args.Request.Message);
            messageDeferral.Complete();
        }

        private void AppServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Debug.WriteLine("[Connection] Service closed");
            _appServiceConnection.Dispose();
            _appServiceConnection = null;
            _appServiceDeferral.Complete();
        }

        public async Task<AppServiceResponse> SendRequestAsync(ValueSet valueSet)
        {
            if (_appServiceConnection == null)
            {
                Debug.WriteLine("[Connection] No active connection");
                return null;
            }

            try
            {
                return await _appServiceConnection.SendMessageAsync(valueSet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Connection] Send message failed: {ex.Message}");
                return null;
            }
        }
    }
}
