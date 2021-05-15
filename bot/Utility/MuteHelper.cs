using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace bot.Utility
{
    public static class MuteHelper
    {
        public static async Task MuteThread(DiscordMember ent, int seconds, DiscordChannel Afk)
        {
            await ent.SetMuteAsync(true);

            while (seconds > 0)
            {
                var chnl = ent.VoiceState?.Channel;
                if (chnl == Afk)
                {
                    Thread.Sleep(5000);
                    continue;
                }

                switch (chnl)
                {
                    case null:
                    {
                        Thread.Sleep(5000);
                        continue;
                    }

                    default:
                    {
                        --seconds;
                        Thread.Sleep(1000);
                        break;
                    }
                }
            }

            await ent.SetMuteAsync(false);
        }
    }
}