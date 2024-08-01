using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinTroy3_PluginLib
{
    public class Control
    {
        public static string GetRandomString(int length = 16)
        {
            string allowedChars = "abcdefghijklmnopqrstuvwxyz";
            StringBuilder sb = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(0, allowedChars.Length);
                sb.Append(allowedChars[index]);
            }
            return sb.ToString();
        }

        public static string[] GetValues(SocketModal modal)
        {
            return modal.Data.Components.Select(component => component.Value).ToArray();
        }

        public static void Log(object value, bool noTime = false)
        {
            if (Config.ClientConfig.IsDebugOutputEnabled())
            {
                if (noTime)
                {
                    Console.WriteLine(value);
                    return;
                }
                Console.WriteLine($"{DateTime.Now:HH:mm:ss} {value}");
            }
        }
    }

    public class ModalResult
    {
        public string ModalId;
        public string DllFileName;
        public string TypeName;
        public string Entry;
        public bool IsModal = true;

        public ModalResult(string modalId, string dllFileName, string typeName, string entry)
        {
            ModalId = modalId;
            DllFileName = dllFileName;
            TypeName = typeName;
            Entry = entry;
        }
    }

    public class Plugin
    {
        public string ButtonLabel;
        public string ButtonId;
        public string DllFileName;
        public string TypeName;
        public string Entry;
        public bool IsMainButton;

        public Plugin(
            string buttonLabel,
            string buttonId,
            string dllFileName,
            string entry = null,
            bool isMainButton = false,
            string typeName = null)
        {
            ButtonLabel = buttonLabel;
            ButtonId = buttonId;
            DllFileName = dllFileName;
            TypeName = typeName;
            Entry = entry;
            IsMainButton = isMainButton;
        }
    }

    public class Respond
    {
        public Modal RespondModal { get; set; }
        public string RespondTitle { get; set; }
        public string RespondText { get; set; }
        public string[] RespondButtons { get; set; }
        public SelectMenuBuilder RespondSelectMenu { get; set; }
        public IEnumerable<FileAttachment> RespondFiles { get; set; }
        public string RespondFile { get; set; }

        public static readonly Respond Empty = new Respond();
    }
}
