using System;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bot.Models;
using DSharpPlus.Entities;

namespace bot.Utility
{
    public static class MuteHelper
    {
        public static async Task MuteThread(DiscordMember ent, int seconds, DiscordChannel Afk, CancellationToken token)
        {
            await ent.SetMuteAsync(true);
            
            while (seconds > 0)
            {
                Console.WriteLine("huj");
                if (!token.IsCancellationRequested)
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
                else
                {
                    await using var context = new DiscordContext();
                    context.Mutes.AddMuteUser(context.Users.GetUserByDiscordMember(ent), seconds);
                    await context.SaveChangesAsync().ConfigureAwait(false);

                    return;
                }
                
            }

            await ent.SetMuteAsync(false);
        }
    }
}