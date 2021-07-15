using System.Threading.Tasks;
using bot.Games.Jackpot;
using bot.Helpers;
using bot.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using static bot.Cmds.MsgHelper;

namespace bot.Cmds
{
    [Group("jackpot")]
    [Description("Komendy do jackopta")]
    [Aliases("j")]
    [Cooldown(1, 1, CooldownBucketType.User)]
    public class JackpotCmds : BaseCommandModule
    {
        [Command("stworz")]
        [Description("Tworzy jackpota")]
        [Aliases("s")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task Create(CommandContext ctx)
        {
            if (!Globals.ENABLE_JACKPOT)    return;
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (Globals.PrintResponseIfNotRightChannel) await WrongChannel(ctx);
                return;
            }


            if (JackpotMain.JackpotRunning)
            {
                await JackpotAlreadyRunning(ctx);
                return;
            }

            new JackpotMain(ctx).Brain();
        }

        [Command("dolacz")]
        [Description("Dolacza do jackpota")]
        [Aliases("d")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task Join(CommandContext ctx, [Description("Ilosc za ile chcesz dolaczyc")]
            int pts)
        {
            if (!Globals.ENABLE_JACKPOT)    return;
            if (ctx.Channel != ctx.Guild.SystemChannel)
            {
                if (Globals.PrintResponseIfNotRightChannel) await WrongChannel(ctx);
                return;
            }


            if (!CheckNumber(pts))
            {
                await WrongNumber(ctx);
                return;
            }

            if (!JackpotMain.JackpotRunning)
            {
                await JackpotNotRunning(ctx);
                return;
            }

            await using (var context = new DiscordContext())
            {
                var dbUser = context.Users.GetUserByDiscordMember(ctx.Member);
                if (!dbUser.HasEnough(pts))
                {
                    await NotEnoughPts(ctx);
                    return;
                }
            }
            
            JackpotMain.JoinJackpot(ctx, pts);
        }
    }
}