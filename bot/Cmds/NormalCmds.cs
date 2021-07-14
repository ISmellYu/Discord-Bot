using System.Threading.Tasks;
using bot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using static bot.Cmds.MsgHelper;
using static bot.Globals;
using static bot.Utility.MuteHelper;

namespace bot.Cmds
{
    [Cooldown(1, 1, CooldownBucketType.User)]
    public class NormalCmds : BaseCommandModule
    {
        [Command("punkty")]
        [Aliases("pkt")]
        [Description("Wyswietla ile mamy punktow")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task ShowPts(CommandContext ctx)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel && ctx.Channel.Name != ROULETTE_CHANNEL_NAME &&
                ctx.Channel.Name != BLACKJACK_CHANNEL_NAME)
            {
                if (PrintResponseIfNotRightChannel) await WrongChannel(ctx);
                return;
            }

            var user = ctx.Member;

            var embed = new DiscordEmbedBuilder
            {
                Description = $"Punkty {user.Mention} wynosza\n**`{DataWrapper.UsersH.GetUser(user).Amount}`**",
                Color = new DiscordColor(0x03fce8)
            };
            embed.WithThumbnail(user.AvatarUrl, 10, 10);
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        [Command("punkty")]
        [Description("Wyswietla ile ktos ma punktow")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task ShowPts(CommandContext ctx, [Description("Uzytkownik")] DiscordMember member)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel && ctx.Channel.Name != ROULETTE_CHANNEL_NAME &&
                ctx.Channel.Name != BLACKJACK_CHANNEL_NAME)
            {
                if (PrintResponseIfNotRightChannel) await WrongChannel(ctx);
                return;
            }

            var user = member;

            var embed = new DiscordEmbedBuilder
            {
                Description = $"Punkty {user.Mention} wynosza\n**`{DataWrapper.UsersH.GetUser(user).Amount}`**",
                Color = new DiscordColor(0x03fce8)
            };
            embed.WithThumbnail(user.AvatarUrl, 10, 10);
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        [Command("przelej")]
        [Description("Wysyla okreslona ilosc punktow do okreslonego uzytkownika")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task GivePts(CommandContext ctx, [Description("Uzytkownik")] DiscordMember ent,
            [Description("Ilosc punktow")] int pts)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (PrintResponseIfNotRightChannel) await WrongChannel(ctx);
                return;
            }

            if (!CheckNumber(pts))
            {
                await WrongNumber(ctx);
                return;
            }


            if (!DataWrapper.UsersH.Exists(ent.Id))
            {
                await UserNotFound(ctx);
                return;
            }

            var user = ctx.Member;
            var dbuser = DataWrapper.UsersH.GetUser(user);
            if (!dbuser.HasEnough(pts))
            {
                await NotEnoughPts(ctx);
                return;
            }

            dbuser.RemovePoints(pts);
            DataWrapper.UsersH.GetUser(ent).AddPoints(pts);

            var emoji = DiscordEmoji.FromName(ctx.Client, ":dollar:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Przelew",
                Description = $"{emoji} {user.Mention} przelał {pts} punktow na konto uzytkownika {ent.Mention}",
                Color = new DiscordColor(0x228B22)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        [Command("mute")]
        [Description("Wycisza uzytkownika na okreslona ilosc minut")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task Mute(CommandContext ctx, [Description("Uzytkownik")] DiscordMember ent,
            [Description("Minuty")] int minutes)
        {
            if (!ENABLE_MUTE)
            {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Nie mozna wyciszyc",
                    Description = $"{emoji} Podany uzytkownik znajduje sie na kanale AFK",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync("", embed: embed);
                return;
            }
            
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (PrintResponseIfNotRightChannel) await WrongChannel(ctx);
                return;
            }

            var chnl = ent.VoiceState?.Channel;
            if (chnl == null)
            {
                await NotOnChannel(ctx);
                return;
            }

            if (ent.VoiceState.IsServerMuted)
            {
                await AlreadyMuted(ctx);
                return;
            }

            if (!CheckNumber(minutes))
            {
                await WrongNumber(ctx);
                return;
            }

            var user = ctx.Member;
            var dbuser = DataWrapper.UsersH.GetUser(user);
            var pts_to_pay = minutes * COST_MUTE;
            if (!dbuser.HasEnough(pts_to_pay))
            {
                await NotEnoughPts(ctx);
                return;
            }

            if (chnl == ctx.Guild.AfkChannel)
            {
                await WrongChannel(ctx);
                return;
            }

            dbuser.RemovePoints(pts_to_pay);
            Muting(ctx, user.Mention, ent.Mention, minutes);
            await MuteThread(ent, minutes * 60, ctx.Guild.AfkChannel);
        }

        [Command("daily")]
        [Description("Odbieranie daily")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task Daily(CommandContext ctx)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (PrintResponseIfNotRightChannel) await WrongChannel(ctx);
                return;
            }

            var dbuser = DataWrapper.UsersH.GetUser(ctx.Member);

            if (!dbuser.Daily)
            {
                await DailyAlreadyClaimed(ctx);
                return;
            }

            await DailyClaimed(ctx);
            dbuser.AddPoints(DAILY_AMOUNT);
            dbuser.Daily = false;
        }

        [Command("ranking")]
        [Description("Wyswietla ranking")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task Rank(CommandContext ctx)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (PrintResponseIfNotRightChannel) await WrongChannel(ctx);
                return;
            }

            var users = DataWrapper.HelpForTypes.GetTopUsers();

            var embed = new DiscordEmbedBuilder
            {
                Title = "Ranking punktow"
            };

            for (var i = 0; i < users.Length; i++)
            {
                var username = await ctx.Guild.GetMemberAsync(users[i].ID).ConfigureAwait(false);
                embed.AddField($"{i + 1}", $"{username} - `{users[i].Amount}`");
            }
                
            await ctx.RespondAsync("", embed: embed);
        }
        
        [Command("rank")]
        [Description("Pokazuje ranking uzytkownika")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task CheckRank(CommandContext ctx)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (PrintResponseIfNotRightChannel) await WrongChannel(ctx);
                return;
            }
            
            var emoji = DiscordEmoji.FromName(ctx.Client, ":crown:");
            var user = ctx.Member;
            var embed = new DiscordEmbedBuilder
            {
                Title = "Miejsce w rankingu",
                Description = $"{emoji} {user.Mention} w rankingu jest na miejscu {DataWrapper.HelpForTypes.GetPlaceForUser(user)}",
                Color = new DiscordColor(0x228B22)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        [Command("przenies")]
        [Description("Prznies uzytkownika na inny kanal")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task Move(CommandContext ctx, [Description("Uzytkownik")] DiscordMember ent,
            [Description("Kanal")] DiscordChannel channel)
        {
            if (!Globals.ENABLE_MOVING)
            {
                return;
            }
            var chnl = ent.VoiceState?.Channel;
            if (chnl == null)
            {
                await NotOnChannel(ctx);
                return;
            }
            var user = DataWrapper.UsersH.GetUser(ctx.Member);
            if (!user.HasEnough(COST_MOVE))
            {
                await NotEnoughPts(ctx);
                return;
            }
            
            
            
        }

        [Command("ping")]
        [Description("Pinguj bota")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync($"Pong: {ctx.Client.Ping}");
        }
    }
}