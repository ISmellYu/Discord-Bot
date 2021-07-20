using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace bot.Models
{
    public class IndividualBet
    {
        public int Id { get; set; }
        [Required]
        public string Option { get; set; }
        [Required]
        public int PlacedPoints { get; set; }
        
        [Required]
        [JsonIgnore]
        public virtual DbUser User { get; set; }
        [Required]
        public virtual Bet Bet { get; set; }
    }
}