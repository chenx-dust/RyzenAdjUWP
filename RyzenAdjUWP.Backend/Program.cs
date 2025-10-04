using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RyzenAdjUWP.Backend
{
    internal static class Program
    {
        static void Main()
        {
            var mutex = new Mutex(true, "RyzenAdjUWP.Backend");
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                Debug.WriteLine("[Mutex] Only one instance at a time");
                return;
            }

            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Debug.WriteLine("[Permission] Should run as Administrator");
                return;
            }

            var comm = new Communication();
            var handler = new Handler();
            handler.Register(comm);
            var task = Task.Run(comm.Run);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Tray());
        }
    }
}
