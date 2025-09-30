using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace RyzenAdjUWP.Backend
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var mutex = new Mutex(true, "RyzenAdjUWP.Backend");
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("[Mutex] Only one instance at a time");
                return;
            }

            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Console.WriteLine("[Permission] Should run as Administrator");
                return;
            }

            var ps = new PipeSecurity();
            var clientRule = new PipeAccessRule(
                new SecurityIdentifier(ApplicationData.Current.LocalSettings.Values["PackageSid"] as string),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow);
            var ownerRule = new PipeAccessRule(
                new SecurityIdentifier(ApplicationData.Current.LocalSettings.Values["UserSid"] as string),
                PipeAccessRights.FullControl,
                AccessControlType.Allow);
            ps.AddAccessRule(clientRule);
            ps.AddAccessRule(ownerRule);

            var pipeName = $"Sessions\\{Process.GetCurrentProcess().SessionId}\\AppContainerNamedObjects\\{ApplicationData.Current.LocalSettings.Values["PackageSid"]}\\RyzenAdjPipe";
            var server = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut, 1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous, 128, 128, ps);

            Console.WriteLine($"[Connection] Waiting for connection from {pipeName}");
            server.WaitForConnection();
            Console.WriteLine($"[Connection] Connection established");
            var reader = new StreamReader(server);
            var writer = new StreamWriter(server) { AutoFlush = true };
            writer.WriteLine("pong");

            while (true)
            {
                if (!server.IsConnected)
                {
                    server.Disconnect();
                    Console.WriteLine("[Connection] Disconnected, waiting for reconnecting");
                    server.WaitForConnection();
                    Console.WriteLine("[Connection] Reconnected");
                    writer.WriteLine("pong");
                }
                string message = reader.ReadLine();
                Console.WriteLine($"[Request] {message}");
            }
        }
    }
}
