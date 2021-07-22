using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace bot.Models
{
    public partial class Bet
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }

        [Required]
        public string FirstOption { get; set; }
        [Required]
        public string SecondOption { get; set; }

        public bool AllowBetting { get; set; } = true;

        [NotMapped] public List<string> Options => new() {FirstOption, SecondOption};

        // [NotMapped]
        // public List<IndividualBet> AllPlacedBets
        // {
        //     get
        //     {
        //         using var context = new DiscordContext();
        //         return context.IndividualBets.GetAllBetsByBetName(Title);
        //     }
        // }
        [JsonIgnore]
        public virtual IList<IndividualBet> AllPlacedBets { get; set; }
        
        [NotMapped]
        public Dictionary<string, double> Odds 
        {
            get
            {
                var bets = AllPlacedBets;

                if (bets.Count == 0)
                {
                    return new Dictionary<string, double>
                    {
                        {FirstOption, 1.00},
                        {SecondOption, 1.00}
                    };
                }
                
                var sumFirstOption = bets.Where(p => p.Option == FirstOption).Sum(p => p.PlacedPoints);
                var sumSecondOption = bets.Where(p => p.Option == SecondOption).Sum(p => p.PlacedPoints);
                var sumAll = Convert.ToDouble(bets.Sum(p => p.PlacedPoints));


                double oddsFirstOption = sumAll / sumFirstOption;
                double oddsSecondOption = sumAll / sumSecondOption;

                if (double.IsInfinity(oddsFirstOption))
                    oddsFirstOption = sumSecondOption;
                
                if (double.IsInfinity(oddsSecondOption))
                    oddsSecondOption = sumFirstOption;
                
                return new Dictionary<string, double>
                {
                    {FirstOption, oddsFirstOption},
                    {SecondOption, oddsSecondOption}
                };
            } 
        }
    }
}