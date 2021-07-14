using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace bot.Games.CoinFlip
{
    public class CoinFlip
    {
        private int _amount;
        private CommandContext ctx;
        private DiscordMember opponent;

        public CoinFlip(CommandContext ctx, DiscordMember opponent, int amount)
        {
        }
    }
}