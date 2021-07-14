using System.Collections.Generic;

namespace bot.Misc
{
    public class ChannelController
    {
    }

    public class PurchasedChannel
    {
        public ulong OwnerId { get; set; }
        public List<ulong> ModsId { get; set; }
        public ulong SpecifiedRoleId { get; set; }
        public string Name { get; set; }
        public int BitRate { get; set; }
        public int UserLimit { get; set; }
        
    }
}