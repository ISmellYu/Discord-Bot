using System;
using System.Linq;
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
                        if (CheckIfSelfMuted(ent) == null)
                        {
                            Thread.Sleep(5000);
                            break;
                        }

                        var checkPrivilege = CheckIfAloneOnChannel(ent);
                        if (CheckIfSelfMuted(ent) == true || checkPrivilege is null or true)
                        {
                            Thread.Sleep(1000);
                            break;
                        }
                        --seconds;
                        Thread.Sleep(1000);
                        break;
                    }
                }
            }

            await ent.SetMuteAsync(false);
        }

        private static bool? CheckIfSelfMuted(DiscordMember ent)
        {
            try
            {
                return ent.VoiceState.IsSelfDeafened;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static bool? CheckIfAloneOnChannel(DiscordMember ent)
        {
            try
            {
                return ent.VoiceState.Channel.Users.Count() > 1;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}