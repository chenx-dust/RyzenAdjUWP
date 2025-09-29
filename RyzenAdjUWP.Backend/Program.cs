using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RyzenAdjUWP.Backend
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!hasAdministrativeRight)
            {
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.Verb = "runas"; // This is the key to requesting elevation
                processInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;

                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    // Handle the case where the user cancels the UAC prompt
                    Console.WriteLine($"Error relaunching with admin rights: {ex.Message}");
                }
                Environment.Exit(0); // Exit the current non-elevated instance
            }
            var backgroundProcess = new BackgroundProcess();
            Console.ReadKey();
        }
    }
}
