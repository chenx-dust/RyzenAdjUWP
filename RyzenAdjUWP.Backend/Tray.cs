using System;
using System.Windows.Forms;
using Windows.System;

namespace RyzenAdjUWP.Backend
{
    internal class Tray : ApplicationContext
    {
        private NotifyIcon _trayIcon;

        public Tray()
        {
            // Initialize Tray Icon
            _trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.TrayIcon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("RyzenAdjUWP.Backend") { Enabled = false },
                    new MenuItem("Exit", Exit),
                }),
                Visible = true
            };
        }

        private void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;

            Application.Exit();
        }
    }
}
