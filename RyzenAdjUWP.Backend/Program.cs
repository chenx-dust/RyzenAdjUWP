using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Storage;

namespace RyzenAdjUWP.Backend
{
    internal static class Program
    {
        const string PROGRAM_NAME = "RyzenAdjUWP.Backend";

        static void Main(string[] args)
        {
            var mutex = new Mutex(true, PROGRAM_NAME);
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

            Console.WriteLine($"{PROGRAM_NAME}");

            string packageSid;
            if (args.Length >= 1 && args[0].StartsWith("S-1-"))
                packageSid = args[0];
            else
                packageSid = ApplicationData.Current.LocalSettings.Values["PackageSid"] as string;
            var comm = new Communication(packageSid);
            var handler = new Handler(RyzenAdj.Open(),
                new AutoStart(PROGRAM_NAME,
                new Microsoft.Win32.TaskScheduler.ExecAction(
                    Assembly.GetEntryAssembly().Location, packageSid
                    )));
            handler.Register(comm);
            var task = Task.Run(comm.Run);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Tray());
        }
    }
}
