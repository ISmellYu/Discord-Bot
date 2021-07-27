using System;
using System.Linq;
using System.Threading.Tasks;
using bot.Helpers;
using bot.Models;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using static bot.Cmds.MsgHelper;

namespace bot.Cmds
{
    [Group("admin")]
    [Description("Komendy admina")]
    [RequireUserPermissions(Permissions.Administrator)]
    public class AdminCmds : BaseCommandModule
    {
        [Command("reset")]
        [Description("Resetuje punkty uzytkownikowi")]
        public async Task Reset(CommandContext ctx, [Description("Uzytkownik")] DiscordMember ent)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (Globals.PrintResponseIfNotRightChannel) await WrongChannelCmd(ctx);
                return;
            }

            await using var context = new DiscordContext();
            context.Users.GetUserByUlong(ent.Id).ResetPoints();
            await context.SaveChangesAsync().ConfigureAwait(false);


            var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Pomyslnie zresetowano!",
                Description = $"{emoji} Zresetowano punkty dla uzytkownika {ent.Mention}",
                Color = new DiscordColor(0x7CFC00)
            };
            await ctx.RespondAsync("", embed: embed);
        }

        [Command("dodaj")]
        [Description("Dodaje punkty uzytkownikowi")]
        public async Task Add(CommandContext ctx, [Description("Uzytkownik")] DiscordMember ent,
            [Description("Ilosc")] int pts)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (Globals.PrintResponseIfNotRightChannel) await WrongChannelCmd(ctx);
                return;
            }

            if (!CheckNumber(pts))
            {
                await WrongNumber(ctx);
                return;
            }
            
            await using var context = new DiscordContext();
            context.Users.GetUserByUlong(ent.Id).AddPoints(pts);
            await context.SaveChangesAsync().ConfigureAwait(false);
            
            var emoji = DiscordEmoji.FromName(ctx.Client, ":plus:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Pomyslnie dodano punkty!",
                Description = $"{emoji} Dodano {pts} punktow uzytkownikowi {ent.Mention}",
                Color = new DiscordColor(0x7CFC00)
            };
            await ctx.RespondAsync("", embed: embed);
        }

        [Command("usun")]
        [Description("Usuwa punkty uzytkownikowi")]
        public async Task Substract(CommandContext ctx, [Description("Uzytkownik")] DiscordMember ent,
            [Description("Ilosc")] int pts)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (Globals.PrintResponseIfNotRightChannel) await WrongChannelCmd(ctx);
                return;
            }

            if (!CheckNumber(pts))
            {
                await WrongNumber(ctx);
                return;
            }

            await using var context = new DiscordContext();
            context.Users.GetUserByUlong(ent.Id).RemovePoints(pts);
            await context.SaveChangesAsync().ConfigureAwait(false);


            var emoji = DiscordEmoji.FromName(ctx.Client, ":minus:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Pomyslnie usunieto punkty!",
                Description = $"{emoji} Usunieto {pts} punktow uzytkownikowi {ent.Mention}",
                Color = new DiscordColor(0xa8a832)
            };
            await ctx.RespondAsync("", embed: embed);
        }

        [Command("wylacz")]
        [Description("Wylacza bezpiecznie bota")]
        public async Task SafelyExit(CommandContext ctx)
        {
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (Globals.PrintResponseIfNotRightChannel) await WrongChannelCmd(ctx);
                return;
            }

            var emoji = DiscordEmoji.FromName(ctx.Client, ":off:");

            var embed = new DiscordEmbedBuilder
            {
                Title = "Wylaczanie bota",
                Description = $"{emoji} Bot zostal wylaczony",
                Color = new DiscordColor(0x03fce8)
            };
            await ctx.RespondAsync("", embed: embed);
            Functions.SafelyExitBot();
        }

        [Command("mute")]
        [Description("Przelacza wlaczenie/wylaczenie mutowania")]
        public async Task TurnMuteOnOff(CommandContext ctx)
        {
            Globals.ENABLE_MUTE = !Globals.ENABLE_MUTE;
            if (Globals.ENABLE_MUTE)
            {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":on:");

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Wylaczanie bota",
                    Description = $"{emoji} Mutowanie zostalo wlaczone",
                    Color = new DiscordColor(0x03fce8)
                };
                await ctx.RespondAsync("", embed: embed);
            }
            else
            {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":off:");

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Wylaczanie mutowania",
                    Description = $"{emoji} Mutowanie zostalo wylaczone",
                    Color = new DiscordColor(0x03fce8)
                };
                await ctx.RespondAsync("", embed: embed);
            }
        }
    }
}