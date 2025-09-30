using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace RyzenAdjUWP.Backend
{
    internal class BackgroundProcess
    {
        private AppServiceConnection Connection { get; set; }

        public async Task InitializeAsync()
        {
            ReInitialize:
            Connection = new AppServiceConnection
            {
                PackageFamilyName = Package.Current.Id.FamilyName,
                AppServiceName = "RyzenAdjAppService"
            };
            AppServiceConnectionStatus status = await Connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                Console.WriteLine("[Connection] Reconnect failed, retrying");
                goto ReInitialize;
            }
            Console.WriteLine("[Connection] Successfully connected to AppService");
            Connection.RequestReceived += Connection_RequestReceived;
            Connection.ServiceClosed += Connection_ServiceClosed;
        }

        private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();
            ValueSet message = null;
            if (args.Request.Message.TryGetValue("cmd", out var cmd))
            {
                switch (cmd)
                {
                    case "set-tdp":
                        Console.WriteLine($"[Request] TDP => {args.Request.Message["tdp"]}");
                        break;
                    case "get-min-tdp":
                        Console.WriteLine($"[Request] Get min tdp");
                        break;
                    case "get-max-tdp":
                        Console.WriteLine($"[Request] Get max tdp");
                        break;
                    case "ping":
                        Console.WriteLine("[Request] Ping pong");
                        message = new ValueSet { { "response", "pong" } };
                        break;
                    case "exit":
                        Console.WriteLine("[Request] Exit command received, exiting");
                        Environment.Exit(0);
                        break;
                }
            }
            if (message != null)
                await Connection.SendMessageAsync(message);
            deferral.Complete();
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Console.WriteLine("[Connection] Closed, waiting for reconnect");
            _ = InitializeAsync();
            //Environment.Exit(0);
        }
    }
}
