using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Media.Protection.PlayReady;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace RyzenAdjUWP
{
    internal class Backend
    {
        public event EventHandler<string> MessageReceivedEvent;
        public event EventHandler ClosedOrFailedEvent;

        private static Backend _instance;
        public static Backend Instance => _instance ?? (_instance = new Backend());

        private NamedPipeClientStream client;
        private StreamReader reader;
        private StreamWriter writer;
        private Task runningThread;

        public bool IsConnected => client.IsConnected;

        private Backend()
        {
            client = new NamedPipeClientStream(".", @"LOCAL\RyzenAdjPipe",
                PipeDirection.InOut, PipeOptions.Asynchronous);

            reader = new StreamReader(client);
            writer = new StreamWriter(client);

            runningThread = Task.Run(Loop);
        }

        public static async Task LaunchBackend()
        {
            ApplicationData.Current.LocalSettings.Values["PackageSid"] = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper();
            ApplicationData.Current.LocalSettings.Values["UserSid"] = WindowsIdentity.GetCurrent().Owner.Value;
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
        }

        private void Loop()
        {
            while (true)
            {
                if (!client.IsConnected)
                {
                    ClosedOrFailedEvent.Invoke(this, null);
                    client.Connect();
                }
                try
                {
                    string message = reader.ReadLine();
                    if (message != null)
                        MessageReceivedEvent.Invoke(this, message);
                }
                catch { }
            }
        }

        public void Send(string message)
        {
            try
            {
                writer.WriteLine(message);
                writer.Flush();
            }
            catch
            {
                ClosedOrFailedEvent.Invoke(this, null);
            }
        }
    }
}
