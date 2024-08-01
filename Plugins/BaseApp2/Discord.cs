using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTroy3_PluginLib;
using static WinTroy3_PluginLib.Config.BotConfig;
using Discord;
using System.Data.Odbc;
using System.Windows.Forms;
using Control = WinTroy3_PluginLib.Control;
using Discord.WebSocket;

public class Dc
{
    public static Respond ShowDiscordMenu()
    {
        return new Respond
        {
            RespondTitle = "Discord",
            RespondText = $"Token: ```{GetToken()}```",
            RespondButtons = new string[]
            {
                "settoken"
            }
        };
    }

    private static Modal GetModalStd(string label)
    {
        var Id = "set" + label.ToLower();
        return new ModalBuilder()
            .WithCustomId($"discordmodal{Id}")
            .WithTitle($"Discord Set {label}")
            .AddTextInput(new TextInputBuilder()
            .WithLabel("New " + label)
            .WithCustomId($"discord{Id}"))
            .Build();
    }

    public static Respond SetToken()
    {
        return new Respond
        {
            RespondModal = GetModalStd("Token")
        };
    }

    public static Respond SetTokenModalSubmit(SocketModal modal)
    {
        var newToken = Control.GetValues(modal)[0];
        Config.BotConfig.SetToken(newToken);
        return new Respond
        {
            RespondText = $"New Token: ```{newToken}```",
            RespondTitle = "Discord Set Token"
        };
    }
}