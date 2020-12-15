using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using static dcBot.Globals;

namespace dcBot.Helpers
{
    public class PtsPerMinute
    {
        public static bool IsRunning = false;
        private readonly DiscordGuild _guild;
        private readonly DiscordRole _role;
        private readonly List<PtsUser> _users = new List<PtsUser>();

        public PtsPerMinute(BaseDiscordClient client)
        {
            Thread.Sleep(5000);
            _guild = client.Guilds.Values.FirstOrDefault();
            _role = _guild.GetRole(ROLE_ID);
            //_role = _guild.GetRole(713860899626287224);
        }

        public async Task MainThread()
        {
            while (true)
            {
                var voiceClients = GetAllVoiceClients().ToArray();
                if (!voiceClients.Any())
                {
                    //Console.WriteLine("No users on voice channels");
                    _users.Clear();
                    await Task.Delay(1000);
                    continue;
                }

                //Remove users that quited channel
                for (var i = _users.Count - 1; i >= 0; i--)
                {
                    var chnl = _users[i].Member.VoiceState?.Channel;
                    if (chnl == null || chnl == _guild.AfkChannel) _users.RemoveAt(i);
                }


                foreach (var discordMember in voiceClients)
                {
                    var exists = _users.SingleOrDefault(p => p.Member == discordMember);
                    if (exists != null)
                    {
                        // Console.WriteLine(
                        //     $"Incrementing {exists.Member.Username} property 'SecondsOnChannel' by 1: {exists.SecondsOnChannel + 1}");
                        exists.SecondsOnChannel += 1;
                        if (exists.SecondsOnChannel % 60 == 0 && exists.Stop == false)
                            //Console.WriteLine($"Adding 1 point to {exists.Member.Username}");
                            exists.Dbuser.AddPoints(1);

                        if (exists.Member.VoiceState.Channel.Users.Count() == 1)
                        {
                            // Console.WriteLine(
                            //     $"Incrementing {exists.Member.Username} property 'SecondsOnChannelAlone' by 1: {exists.SecondsOnChannelAlone + 1}");
                            exists.SecondsOnChannelAlone += 1;
                            if (exists.SecondsOnChannelAlone % 600 != 0) continue;
                            //Console.WriteLine($"Settings {exists.Member.Username} property 'stop' to true");
                            exists.Stop = true;
                        }
                        else
                        {
                            // Console.WriteLine(
                            //     $"Resseting {exists.Member.Username} property 'SecondsOnChannelAlone' to 0");
                            exists.SecondsOnChannelAlone = 0;
                            //Console.WriteLine($"Settings {exists.Member.Username} property 'stop' to false");
                            exists.Stop = false;
                        }
                    }
                    else
                    {
                        //Console.WriteLine($"Adding {discordMember.Username}");
                        _users.Add(new PtsUser(discordMember, DataWrapper.UsersH.GetUser(discordMember)));
                    }
                }

                //Console.WriteLine("-------------------------------------------");
                await Task.Delay(1000);
            }
        }

        private IEnumerable<DiscordMember> GetAllVoiceClients()
        {
            return _guild.Channels.Values.Where(p => p.Type == ChannelType.Voice && p != _guild.AfkChannel)
                .SelectMany(client => client.Users).Where(p => p.Roles.Contains(_role));
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