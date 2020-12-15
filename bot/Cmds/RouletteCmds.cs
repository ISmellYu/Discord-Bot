using System;
using System.Threading.Tasks;
using dcBot.Games.Roulette;
using dcBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using static dcBot.Cmds.MsgHelper;
using static dcBot.Globals;

namespace dcBot.Cmds
{
    [Group("ruletka")]
    [Description("Komendy do ruletki")]
    [Aliases("r")]
    [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
    [Cooldown(1, 1, CooldownBucketType.User)]
    public class RouletteCmds : BaseCommandModule
    {
        [Command("stworz")]
        [Aliases("s")]
        [RequireOwner]
        public async Task Create(CommandContext ctx)
        {
            if (ctx.Channel.Name != ROULETTE_CHANNEL_NAME)
            {
                await WrongChannelCmd(ctx);
                return;
            }

            if (Roulette._running)
            {
                await RouletteAlreadyRunning(ctx);
                return;
            }

            Task.Run(() => new Roulette(ctx).Main());
        }

        [Command("wylacz")]
        [Aliases("w")]
        [RequireOwner]
        public async Task Exit(CommandContext ctx)
        {
            if (ctx.Channel.Name != ROULETTE_CHANNEL_NAME)
            {
                await WrongChannelCmd(ctx);
                return;
            }

            if (!Roulette._running)
            {
                await RouletteNotRunning(ctx);
                return;
            }

            Roulette._can_run = false;
        }

        [Command("postaw")]
        [Aliases("p")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        [Priority(1)]
        public async Task Place(CommandContext ctx, [Description("Kolor")] string color,
            [Description("Ilosc")] int amount)
        {
            if (ctx.Channel.Name != ROULETTE_CHANNEL_NAME)
            {
                await WrongChannelCmd(ctx);
                return;
            }

            if (!"rgb".Contains(color)) throw new ArgumentException();

            if (!CheckNumber(amount))
            {
                await WrongNumber(ctx);
                return;
            }

            if (!Roulette._running)
            {
                await RouletteNotRunning(ctx);
                return;
            }

            if (!DataWrapper.UsersH.GetUser(ctx.Member).HasEnough(amount))
            {
                await NotEnoughPts(ctx);
                return;
            }

            await Roulette.JoinRoulette(ctx, Convert.ToChar(color), amount);
        }

        [Command("postaw")]
        [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
        public async Task Place(CommandContext ctx, [Description("Kolor")] string color,
            [Description("Ilosc")] string all)
        {
            if (ctx.Channel.Name != ROULETTE_CHANNEL_NAME)
            {
                await WrongChannelCmd(ctx);
                return;
            }

            if (!"all".Contains(all)) throw new ArgumentException();

            if (!Roulette._running)
            {
                await RouletteNotRunning(ctx);
                return;
            }

            var user = DataWrapper.UsersH.GetUser(ctx.Member).Amount;
            if (user == 0)
            {
                await NotEnoughPts(ctx);
                return;
            }

            await Roulette.JoinRoulette(ctx, Convert.ToChar(color), user);
        }
    }
}