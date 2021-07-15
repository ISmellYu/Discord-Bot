using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace bot.Models
{
    public class Option
    {
        public int Id { get; set; }
        [Required]
        public string OptionName { get; set; }
        
        public List<Bet>? Bet { get; set; }
        public List<IndividualBet>? IndividualBet { get; set; }
    }
}