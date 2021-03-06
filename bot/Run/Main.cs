using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bot.Utility;
using bot.Cmds;
using bot.Helpers;
using bot.Models;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace bot.Run
{
    public class Bot
    {
        private readonly EventId BotEventId = new(42, "JSZ Bot");
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
                #if !DEBUG
                Token = cfgjson.Token,
                #else
                Token = "NzEzODYxNjYwMTU4NDU5OTA0.XsmReg.BlVxz0KwTPvIw5WasXY_hvCkK14",  // Expired btw dont try xD
                #endif
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };

            // then we want to instantiate our client
            Client = new DiscordClient(cfg);

            

            // next, let's hook some events, so we know
            // what's going on
            Client.Ready += OnClientReady;
            Client.GuildMemberRemoved += OnClientLeave;
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
            // up next, let's set up our commands
            var ccfg = new CommandsNextConfiguration
            {
                // let's use the string prefix defined in config.json
                StringPrefixes = new[] {"!"},

                // enable responding in direct messages
                EnableDms = true,

                // enable mentioning the bot as a command prefix
                EnableMentionPrefix = false
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
        
        private Task OnClientLeave(DiscordClient dc, GuildMemberRemoveEventArgs e)
        {
            // TODO: Manage it to work xD
            // if (e.Member.Roles.SingleOrDefault(p => p.Id == Globals.ROLE_ID) == null)
            //     return Task.CompletedTask;
            //
            // using (var context = new DiscordContext())
            // {
            //     var user = context.Users.GetUserByUlong(e.Member.Id);
            //     if (user == null)
            //         return Task.CompletedTask;
            //     
            //     context.Users.RemoveUser(user);
            //     context.SaveChanges();
            // }
            
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

            //File.WriteAllText("log.txt", $"{e.Exception}");

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