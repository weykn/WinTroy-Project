using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinTroy3_Client.Menu
{
    public class FollowupModal
    {
        public class ReplyTo
        {
            public SocketInteraction Interaction;
            public SocketUserMessage Message;
            public bool FollowupInteraction = false;

            public ReplyTo(
                SocketInteraction interaction = null,
                SocketUserMessage message = null,
                bool followupInteraction = false)
            {
                Interaction = interaction;
                Message = message;
                FollowupInteraction = followupInteraction;
            }

            public async Task Reply(
                Embed embed,
                MessageComponent components = null)
            {
                if (Interaction == null)
                    await Message.ReplyAsync(
                        components: components,
                        embed: embed);
                else
                {
                    if (FollowupInteraction)
                        await Interaction.FollowupAsync(
                            components: components,
                            embed: embed);
                    else
                        await Interaction.RespondAsync(
                            components: components,
                            embed: embed);
                }
            }
        }

        public static Modal followupModal;

        public static async Task Show(
            Modal modal,
            SocketInteraction interaction = null,
            SocketUserMessage message = null)
        {
            followupModal = modal;
            var components = new ComponentBuilder()
                .WithButton("Show Modal", "showfollowupmodal")
                .Build();
            var embed = new EmbedBuilder()
                .WithDescription("The clicked button would like to display a modal")
                .WithTitle("Modal")
                .WithColor(Color.Green)
                .Build();
            await new ReplyTo(interaction, message, true).Reply(embed, components);
        }
    }
}
