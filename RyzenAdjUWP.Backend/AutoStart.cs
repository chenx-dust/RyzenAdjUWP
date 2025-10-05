using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.System;

namespace RyzenAdjUWP.Backend
{
    internal class AutoStart
    {
        public string name { get; private set; }
        private TaskDefinition _taskDefinition;

        public AutoStart(string name, ExecAction action)
        {
            this.name = name;
            _taskDefinition = TaskService.Instance.NewTask();
            _taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;
            _taskDefinition.Principal.UserId = WindowsIdentity.GetCurrent().Name;
            _taskDefinition.Principal.LogonType = TaskLogonType.InteractiveToken;
            _taskDefinition.Settings.DisallowStartIfOnBatteries = false;
            _taskDefinition.Settings.StopIfGoingOnBatteries = false;
            _taskDefinition.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            _taskDefinition.Settings.Enabled = false;
            _taskDefinition.Triggers.Add(new LogonTrigger() { UserId = WindowsIdentity.GetCurrent().Name });
            _taskDefinition.Actions.Add(action);
        }

        public void SetEnabled(bool enabled)
        {
            try
            {
                // get current task, if any, delete it
                var task = TaskService.Instance.FindTask(name);
                if (task != null)
                    TaskService.Instance.RootFolder.DeleteTask(name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AutoStart] Failed to delete existing task {name}: {ex}");
            }
            if (enabled)
            {
                var task = TaskService.Instance.RootFolder.RegisterTaskDefinition(name, _taskDefinition);
                task.Enabled = true;
            }
        }

        public static bool IsEnabled(string name)
        {
            return TaskService.Instance.FindTask(name)?.Enabled ?? false;
        }
    }
}
