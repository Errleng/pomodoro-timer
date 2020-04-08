using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Pomodoro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, Int32 nCmdShow);

        protected override void OnStartup(StartupEventArgs e)
        {
            Process curProc = Process.GetCurrentProcess();
            Process existingProc = null;

            foreach (Process proc in Process.GetProcesses())
            {
                // perhaps a better way is to use a mutex
                // however, bringing the window into focus will require extern methods such as wndProc
                if (!proc.Id.Equals(curProc.Id) && proc.ProcessName.Equals(curProc.ProcessName))
                {
                    existingProc = proc;
                    break;
                }
            }

            if (existingProc != null)
            {
                // magic number
                ShowWindow(existingProc.MainWindowHandle, 9);
                Current.Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
