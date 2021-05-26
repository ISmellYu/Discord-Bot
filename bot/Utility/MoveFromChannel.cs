using DSharpPlus.Entities;

namespace bot.Utility
{
    public static class MoveFromChannel
    {
        public static async void MoveThread(DiscordMember mem, DiscordChannel channel)
        {
            await mem.PlaceInAsync(channel).ConfigureAwait(false);
        }
    }
}