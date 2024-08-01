using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTroy3_PluginLib
{
    public class CmdControl
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

        static public CmdResult Cmd(string command)
        {
            var startInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = "/c " + command,
                FileName = "cmd.exe"
            };
            var p = Process.Start(startInfo);
            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();
            p.WaitForExit();
            return new CmdResult(error, output, command);
        }
    }
}
