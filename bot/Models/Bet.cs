using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace bot.Models
{
    public class Bet
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }

        public List<Option> Options { get; set; }
        public List<IndividualBet> AllPlacedBets { get; set; }
    }
}