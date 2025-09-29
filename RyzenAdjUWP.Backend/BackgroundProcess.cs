using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace RyzenAdjUWP.Backend
{
    internal class BackgroundProcess
    {
        private AppServiceConnection Connection { get; set; }

        public Task InitializeTask { get; private set; }

        public BackgroundProcess()
        {
            InitializeTask = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            Connection = new AppServiceConnection();
            Connection.PackageFamilyName = Package.Current.Id.FamilyName;
            Connection.AppServiceName = "RyzenAdjAppService";
            AppServiceConnectionStatus status = await Connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                Console.WriteLine("[Connection] Successfully connected to AppService");
                throw new Exception(status.ToString());
            }
            Console.WriteLine(status);
            Connection.RequestReceived += Connection_RequestReceived;
            Connection.ServiceClosed += Connection_ServiceClosed;
        }

        private void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            var message = new ValueSet();
            Console.WriteLine($"[Request] TDP => {args.Request.Message["TDP"]}");
            //message.Add("response", $"Received request: {args.Request}");
            //await Connection.SendMessageAsync(message);
            deferral.Complete();
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Environment.Exit(0);
        }
    }
}
