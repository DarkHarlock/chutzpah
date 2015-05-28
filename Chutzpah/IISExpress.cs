using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Management;

namespace Chutzpah
{
    public interface IIISExpressFactory
    {
        IDisposable Create(string cmdLine, bool stopServerOnDispose);
    }

    public class IISExpressFactory : IIISExpressFactory
    {
        private class IISExpress : IDisposable
        {
            private class NativeMethods
            {
                // Methods
                [DllImport("user32.dll", SetLastError = true)]
                public static extern IntPtr GetTopWindow(IntPtr hWnd);
                [DllImport("user32.dll", SetLastError = true)]
                public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
                [DllImport("user32.dll", SetLastError = true)]
                public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);
                [DllImport("user32.dll", SetLastError = true)]
                public static extern bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
            }

            private static bool SendStopMessageToProcess(int PID)
            {
                try
                {
                    for (IntPtr ptr = NativeMethods.GetTopWindow(IntPtr.Zero); ptr != IntPtr.Zero; ptr = NativeMethods.GetWindow(ptr, 2))
                    {
                        uint num;
                        NativeMethods.GetWindowThreadProcessId(ptr, out num);
                        if (PID == num)
                        {
                            HandleRef hWnd = new HandleRef(null, ptr);
                            NativeMethods.PostMessage(hWnd, 0x12, IntPtr.Zero, IntPtr.Zero);
                            return true;
                        }
                    }
                }
                catch { }
                return false;
            }

            private readonly Process process;
            private readonly int PID = 0;
            private readonly bool stopServerOnDispose;

            private static string GetCommandLine(Process process)
            {
                using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
                {
                    foreach (var @object in searcher.Get())
                    {
                        return @object["CommandLine"].ToString();
                    }
                }
                return string.Empty;
            }

            public IISExpress(string executable, string cmdLine, bool stopServerOnDispose)
            {
                this.stopServerOnDispose = stopServerOnDispose;
                try
                {
                    var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exe))
                        .Where(p => GetCommandLine(p).Contains(cmdLine.Trim()))
                        .ToList();

                    if (processes.Any())
                    {
                        process = processes.First();
                    }
                    else
                    {
                        process = Process.Start(new ProcessStartInfo
                        {
                            FileName = executable,
                            Arguments = cmdLine,
                            RedirectStandardOutput = false,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        });
                        PID = process.Id;
                    }
                }
                catch { }
            }

            private bool disposed = false;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposed && stopServerOnDispose)
                {
                    if (PID != 0)
                    {
                        SendStopMessageToProcess(PID);
                        if (disposing)
                        {
                            //process.WaitForExit(10000);
                            process.Dispose();
                        }
                    }
                    disposed = true;
                }
            }
            ~IISExpress()
            {
                Dispose(false);
            }
        }

        private const string exe = "iisexpress.exe";
        private const string x32 = @"C:\Program Files\IIS Express";
        private const string x64 = @"C:\Program Files (x86)\IIS Express";

        private readonly string path;
        public IISExpressFactory()
        {
            var exe1 = Path.Combine(x32, exe);
            var exe2 = Path.Combine(x64, exe);
            path = File.Exists(exe1) ? exe1 : exe2;
        }
        public IDisposable Create(string cmdLine, bool stopServerOnDispose)
        {
            return new IISExpress(path, cmdLine, stopServerOnDispose);
        }
    }
}
