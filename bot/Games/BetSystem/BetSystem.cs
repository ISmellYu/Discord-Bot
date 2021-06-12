using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;

namespace dcBot.Games
{
    public class BetSystem
    {
        public List<Bet> Bets { get; init; }
        public BetSystem()
        {
            Bets = new List<Bet>();
        }

        public bool CreateBet(string name, string option1, string option2)
        {
            
            return false;

        }

        public bool JoinBet(string name, string option, DiscordMember user, int amount)
        {
            return false;

        }

        public bool EndBet(string name, string option)
        {
            return false;
        }


        public class Bet
        {
            public string Name { get; }
            private List<string> _options;
            public IReadOnlyList<string> Options => _options;
            private Dictionary<string, double> Odds
            {
                get
                {
                    return new Dictionary<string, double>();
                }
            }
            public List<IndividualBet> Players { get; set; }
            public Bet(string name, List<string> options)
            {
                Name = name;
                _options = options;
            }
            
            public Bet(string name, string option1, string option2)
            {
                Name = name;
                _options = new List<string> {option1, option2};
            }
        }

        public class IndividualBet
        {
            public DiscordMember User { get; init; }
            public string Option { get; init; }
            public int Amount { get; set; }
        }
    }
}