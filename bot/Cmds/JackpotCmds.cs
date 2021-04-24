using System.Threading.Tasks;
using dcBot.Games.Jackpot;
using dcBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using static dcBot.Cmds.MsgHelper;

namespace dcBot.Cmds
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

            var dbuser = DataWrapper.UsersH.GetUser(ctx.Member);
            if (!dbuser.HasEnough(pts))
            {
                await NotEnoughPts(ctx);
                return;
            }

            JackpotMain.JoinJackpot(ctx, dbuser, pts);
        }
    }
}