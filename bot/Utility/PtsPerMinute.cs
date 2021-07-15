using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bot.Helpers;
using bot.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using static bot.Globals;

namespace bot.Utility
{
    public class PtsPerMinute
    {
        public static bool IsRunning = false;
        private static DiscordGuild _guild;
        private readonly DiscordRole _role;
        private readonly List<VoiceUser> _voiceUsers = new();

        public PtsPerMinute(BaseDiscordClient client)
        {
            Thread.Sleep(5000);
            _guild = client.Guilds.Values.FirstOrDefault();
            if (_guild is not null) _role = _guild.GetRole(ROLE_ID);
            //_role = _guild.GetRole(713860899626287224);
        }

        public async Task MainThread()
        {
            while (true)
            {
                var voiceClients = GetAllVoiceClients();

                if (!voiceClients.Any())
                {
                    CleanUpAndDelay(5000);
                    continue;
                }

                foreach (var member in voiceClients)
                {
                    var voiceUser = _voiceUsers.SingleOrDefault(p => p.Id == member.Id);

                    if (voiceUser != null)
                    {
                        IncrementSecondsAndAddPointsIfNeeded(PTS_PER_MINUTE_INTERVAL_SECONDS, PTS_PER_MINUTE_AMOUNT,
                            PTS_PER_MINUTE_AMOUNT_BOOSTER, member, voiceUser);
                    }
                    else
                    {
                        _voiceUsers.Add(new VoiceUser(member.Id));
                    }
                }

                await Task.Delay(1000);
            }
        }

        private IEnumerable<DiscordMember> GetAllVoiceClients()
        {
            return _guild.Channels.Values.Where(p => p.Type == ChannelType.Voice && p != _guild.AfkChannel)
                .SelectMany(client => client.Users).Where(p => p.Roles.Contains(_role) && !p.IsBot);
        }

        private async void CleanUpAndDelay(int delay)
        {
            _voiceUsers.Clear();
            await Task.Delay(delay);
        }
    
        private async void IncrementSecondsAndAddPointsIfNeeded(int intervalSeconds, int pointsToAdd, 
            int pointsToAddBooster, DiscordMember member, VoiceUser user)
        {
            user.IncrementSeconds(1);
            if (user.Seconds % intervalSeconds != 0) return;
            await using var context = new DiscordContext();
            var dbUser = context.Users.GetUserByUlong(user.Id);
            var isBooster = member.PremiumSince != null;
            dbUser.AddPoints(isBooster ? pointsToAddBooster : pointsToAdd);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        private class VoiceUser
        {
            public ulong Id { get; set; }
            public int Seconds { get; set; } = 0;

            public VoiceUser(ulong id)
            {
                Id = id;
            }

            public void IncrementSeconds(int seconds)
            {
                Seconds += seconds;
            }

            public async Task<bool> IsOnChannelAndNotMuted()
            {
                var member = await _guild.GetMemberAsync(Id).ConfigureAwait(false);
                try
                {
                    var vs = member.VoiceState;
                    return !vs.IsSelfDeafened || !vs.IsSelfMuted;
                }
                catch (NullReferenceException)
                {
                    return false;
                }
            }
        }
        
    }
}