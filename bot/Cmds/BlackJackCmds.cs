using System.Threading.Tasks;
using dcBot.Games.BlackJack;
using dcBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using static dcBot.Cmds.MsgHelper;

namespace dcBot.Cmds
{
    [Group("blackjack")]
    [Description("Komendy do blacjacka")]
    [Aliases("b")]
    [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
    [Cooldown(1, 1, CooldownBucketType.User)]
    public class BlackJackCmds : BaseCommandModule
    {
        [Command("stworz")]
        [Description("Tworzy blackjacka")]
        [Aliases("s")]
        public async Task Create(CommandContext ctx, [Description("Uzytkownik z ktorym chcesz zagrac")]
            DiscordMember opponent, [Description("Ilosc punktow ktore chcesz postawic")]
            int amount)
        {
            if (BlackJack._isRunning || ctx.Member == opponent) return;

            if (ctx.Channel.Name != Globals.BLACKJACK_CHANNEL_NAME)
            {
                await WrongChannel(ctx);
                return;
            }

            if (!CheckNumber(amount))
            {
                await WrongNumber(ctx);
                return;
            }

            var user = DataWrapper.UsersH.GetUser(ctx.Member);
            if (!user.HasEnough(amount))
            {
                await NotEnoughPts(ctx);
                return;
            }

            user.RemovePoints(amount);
            new BlackJack(ctx, opponent, amount).Brain();
        }
    }
}