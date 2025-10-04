using System;
using System.Security.Principal;
using System.Threading;

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

            var comm = new Communication();
            var handler = new Handler();
            handler.Register(comm);
            comm.Run();
        }
    }
}
