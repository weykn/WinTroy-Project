using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using WinTroy3_PluginLib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using static WinTroy3_Client.Menu.Plugins;

namespace WinTroy3_Client.Menu
{
    public class SelectMenuHandler
    {
        public static Task SelectMenuExecutedSync(SocketMessageComponent arg)
        {
            Reg.SetDataString(arg.Data.CustomId, arg.Data.Values.ElementAt(0));
            arg.DeferAsync();
            return Task.CompletedTask;
        }
    }
}
