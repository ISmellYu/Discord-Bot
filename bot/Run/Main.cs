using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using dcBot.Cmds;
using dcBot.Helpers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace dcBot.Run
{
    public class Bot
    {
        private readonly EventId BotEventId = new(42, "Bot-Ex03");
        private DiscordClient Client { get; set; }
        public InteractivityExtension Interactivity { get; set; }
        private CommandsNextExtension Commands { get; set; }

        public async Task RunAsync()
        {
            // first, let's load our configuration file
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                json = await sr.ReadToEndAsync();
            }

            // next, let's load the values from that file
            // to our client's configuration
            var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
            var cfg = new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };

            // then we want to instantiate our client
            Client = new DiscordClient(cfg);

            // If you are on Windows 7 and using .NETFX, install 
            // DSharpPlus.WebSocket.WebSocket4Net from NuGet,
            // add appropriate usings, and uncomment the following
            // line
            //this.Client.SetWebSocketClient<WebSocket4NetClient>();

            // If you are on Windows 7 and using .NET Core, install 
            // DSharpPlus.WebSocket.WebSocket4NetCore from NuGet,
            // add appropriate usings, and uncomment the following
            // line
            //this.Client.SetWebSocketClient<WebSocket4NetCoreClient>();

            // If you are using Mono, install 
            // DSharpPlus.WebSocket.WebSocketSharp from NuGet,
            // add appropriate usings, and uncomment the following
            // line
            //this.Client.SetWebSocketClient<WebSocketSharpClient>();

            // if using any alternate socket client implementations, 
            // remember to add the following to the top of this file:
            //using DSharpPlus.Net.WebSocket;

            // next, let's hook some events, so we know
            // what's going on
            Client.Ready += OnClientReady;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientError;

            // let's enable interactivity, and set default options
            Client.UseInteractivity(new InteractivityConfiguration
            {
                // default pagination behaviour to just ignore the reactions
                PaginationBehaviour = PaginationBehaviour.Ignore,

                // default timeout for other actions to 2 minutes
                Timeout = TimeSpan.FromMinutes(2)
            });
            //Halo
            // up next, let's set up our commands
            var ccfg = new CommandsNextConfiguration
            {
                // let's use the string prefix defined in config.json
                StringPrefixes = new[] {"jsz"},

                // enable responding in direct messages
                EnableDms = true,

                // enable mentioning the bot as a command prefix
                EnableMentionPrefix = true
            };

            // and hook them up
            Commands = Client.UseCommandsNext(ccfg);

            // let's hook some command events, so we know what's 
            // going on
            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            // up next, let's register our commands
            //this.Commands.RegisterCommands<Commands>();
            Commands.RegisterCommands<AdminCmds>();
            Commands.RegisterCommands<NormalCmds>();
            Commands.RegisterCommands<JackpotCmds>();
            Commands.RegisterCommands<BlackJackCmds>();
            // finally, let's connect and log in
            await Client.ConnectAsync();

            // when the bot is running, try doing <prefix>help
            // to see the list of registered commands, and 
            // <prefix>help <command> to see help about specific
            // command.

            // Run reset daily thread
            Task.Run(() => Daily.DailyThread());

            //Run ptsperminute
            Task.Run(() => new PtsPerMinute(Client).MainThread());

            // and this is to prevent premature quitting
            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient dc, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            // let's log the details of the error that just 
            // occured in our client
            sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");

            File.WriteAllText("log.txt", $"{e.Exception}");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.Logger.LogInformation(BotEventId,
                $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            // let's log the error details
            e.Context.Client.Logger.LogError(BotEventId,
                $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}",
                DateTime.Now);
            
            if (e.Context.Channel.Name != Globals.BOT_CHANNEL_NAME || e.Context.Channel.Name != Globals.BLACKJACK_CHANNEL_NAME)
            {
                return;
            }


            switch (e.Exception)
            {
                case ChecksFailedException _: // Permissions
                {
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Brak dostepu!",
                        Description = $"{emoji} Nie masz wystarczajacych uprawnien aby wykonac komende",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await e.Context.RespondAsync("", embed: embed);
                    break;
                }

                case CommandNotFoundException _: // Command not found
                {
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":grey_question:");

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Brak komendy!",
                        Description = $"{emoji} Wybrana komenda nie istnieje",
                        Color = new DiscordColor(0xC0C0C0)
                    };
                    await e.Context.RespondAsync("", embed: embed);
                    break;
                }

                case ArgumentException _: // Wrong arguments
                {
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":x:");

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Zle argumenty",
                        Description =
                            $"{emoji} Wpisales zle argumenty do komendy, uzyj help aby zobaczyc argumenty do wybranej komendy",
                        Color = new DiscordColor(0xC0C0C0)
                    };
                    await e.Context.RespondAsync("", embed: embed);
                    break;
                }
            }
        }
    }
}