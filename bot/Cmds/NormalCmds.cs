using System.Threading.Tasks;
using dcBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using static dcBot.Cmds.MsgHelper;
using static dcBot.Globals;
using static dcBot.Helpers.MuteHelper;

namespace dcBot.Cmds
{
    [Cooldown(1, 1, CooldownBucketType.User)]
    public class NormalCmds : BaseCommandModule
    {
        [Command("punkty")]
        [Description("Wyswietla ile mamy punktow")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task ShowPts(CommandContext ctx)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel && ctx.Channel.Name != ROULETTE_CHANNEL_NAME &&
                ctx.Channel.Name != BLACKJACK_CHANNEL_NAME)
            {
                if (Globals.PrintResponseIfNotRightChannel)
                {
                    await WrongChannel(ctx);
                }
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
        [Description("Wyswietla ile mamy punktow")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task ShowPts(CommandContext ctx, [Description("Uzytkownik")] DiscordMember member)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel && ctx.Channel.Name != ROULETTE_CHANNEL_NAME &&
                ctx.Channel.Name != BLACKJACK_CHANNEL_NAME)
            {
                if (Globals.PrintResponseIfNotRightChannel)
                {
                    await WrongChannel(ctx);
                }
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

        [Command("przelej")]
        [Description("Wysyla okreslona ilosc punktow do okreslonego uzytkownika")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task GivePts(CommandContext ctx, [Description("Uzytkownik")] DiscordMember ent,
            [Description("Ilosc punktow")] int pts)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (Globals.PrintResponseIfNotRightChannel)
                {
                    await WrongChannel(ctx);
                }
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
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (Globals.PrintResponseIfNotRightChannel)
                {
                    await WrongChannel(ctx);
                }
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
            await Muting(ctx, user.Mention, ent.Mention, minutes);
            await MuteThread(ent, minutes * 60, ctx.Guild.AfkChannel);
        }

        [Command("daily")]
        [Description("Odbieranie daily")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task Daily(CommandContext ctx)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (Globals.PrintResponseIfNotRightChannel)
                {
                    await WrongChannel(ctx);
                }
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
                if (Globals.PrintResponseIfNotRightChannel)
                {
                    await WrongChannel(ctx);
                }
                return;
            }
            var users = DataWrapper.HelpForTypes.GetTopUsers();

            var embed = new DiscordEmbedBuilder
            {
                Title = "Ranking punktow"
            };

            for (var i = 0; i < users.Length; i++) embed.AddField($"{i + 1}", $"{users[i].User} - `{users[i].Amount}`");
            await ctx.RespondAsync("", embed: embed);
        }
    }
}