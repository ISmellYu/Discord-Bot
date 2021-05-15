using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dcBot.Helpers;
using DSharpPlus;
using DSharpPlus.Entities;
using static dcBot.Globals;

namespace bot.Utility
{
    public class PtsPerMinute
    {
        public static bool IsRunning = false;
        private readonly DiscordGuild _guild;
        private readonly DiscordRole _role;
        private readonly List<PtsUser> _users = new();

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
                var voiceClients = GetAllVoiceClients().ToArray();
                if (!voiceClients.Any())
                {
                    _users.Clear();
                    await Task.Delay(1000);
                    continue;
                }

                //Remove users that quited channel
                for (var i = 0; i < _users.Count; i++)
                {
                    var chnl = _users[i].Member.VoiceState?.Channel;
                    if (chnl == null || chnl == _guild.AfkChannel) _users.RemoveAt(i);
                }


                foreach (var discordMember in voiceClients)
                {
                    var exists = _users.SingleOrDefault(p => p.Member == discordMember);
                    if (exists != null)
                    {
                        exists.SecondsOnChannel += 1;
                        if (exists.SecondsOnChannel == 60 && exists.Stop == false)
                        {
                            exists.Dbuser.AddPoints(3);
                            exists.SecondsOnChannel = 0;
                        }

                        // if (exists.Member.VoiceState.Channel.Users.Count() == 1)
                        // {
                        //     exists.SecondsOnChannelAlone += 1;
                        //     if (exists.SecondsOnChannelAlone % 600 != 0) continue;
                        //     exists.Stop = true;
                        // }
                        // else
                        // {
                        //     exists.SecondsOnChannelAlone = 0;
                        //
                        //     exists.Stop = false;
                        // }
                    }
                    else
                    {
                        _users.Add(new PtsUser(discordMember, DataWrapper.UsersH.GetUser(discordMember)));
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

        private class PtsUser
        {
            public readonly DbUser Dbuser;
            public readonly DiscordMember Member;
            public int SecondsOnChannel;
            public int SecondsOnChannelAlone;
            public bool Stop;

            public PtsUser(DiscordMember user2, DbUser user)
            {
                Dbuser = user;
                Member = user2;
                SecondsOnChannel = 0;
                SecondsOnChannelAlone = 0;
            }
        }
    }
}