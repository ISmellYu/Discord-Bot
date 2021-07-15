using System.Threading.Tasks;
using bot.Games.BlackJack;
using bot.Helpers;
using bot.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using static bot.Cmds.MsgHelper;

namespace bot.Cmds
{
    [Group("blackjack")]
    [Description("Komendy do blacjacka")]
    [Aliases("b")]
    [RequireRoles(RoleCheckMode.Any, "Zweryfikowany")]
    [Cooldown(1, 2, CooldownBucketType.User)]
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
                if (Globals.PrintResponseIfNotRightChannel) await WrongChannel(ctx);

                return;
            }
            
            if (!CheckNumber(amount))
            {
                await WrongNumber(ctx);
                return;
            }

            await using (var context = new DiscordContext())
            {
                if (!context.Users.CheckIfExists(opponent))
                {
                    await UserNotFound(ctx);
                    return;
                }

                var user = context.Users.GetUserByDiscordMember(ctx.Member);
                if (!user.HasEnough(amount))
                {
                    await NotEnoughPts(ctx);
                    return;
                }
                user.RemovePoints(amount);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            
            new BlackJack(ctx, opponent, amount).Brain();
        }
    }
}