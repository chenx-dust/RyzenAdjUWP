using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.Authentication.Web;
using Windows.Storage;

namespace RyzenAdjUWP
{
    internal class Backend
    {
        public event EventHandler<string> MessageReceivedEvent;
        public event EventHandler ClosedOrFailedEvent;

        private static Backend _instance;
        public static Backend Instance => _instance ?? (_instance = new Backend());

        private NamedPipeClientStream _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private Task _loop;

        public bool IsConnected => _client.IsConnected;

        private Backend()
        {
            _client = new NamedPipeClientStream(".", @"LOCAL\RyzenAdjPipe",
                PipeDirection.InOut, PipeOptions.Asynchronous);

            _reader = new StreamReader(_client);
            _writer = new StreamWriter(_client);

            _loop = Task.Run(Loop);
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
                if (!_client.IsConnected)
                {
                    ClosedOrFailedEvent?.Invoke(this, null);
                    _client.Connect();
                }
                try
                {
                    string message = _reader.ReadLine();
                    if (message != null)
                        MessageReceivedEvent?.Invoke(this, message);
                }
                catch { }
            }
        }

        public void Send(string message)
        {
            try
            {
                lock (_writer)
                {
                    _writer.WriteLine(message);
                    _writer.Flush();
                }
            }
            catch
            {
                ClosedOrFailedEvent?.Invoke(this, null);
            }
        }
    }
}
