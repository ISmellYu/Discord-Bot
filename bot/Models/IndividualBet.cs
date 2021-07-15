using System.ComponentModel.DataAnnotations;

namespace bot.Models
{
    public class IndividualBet
    {
        public int Id { get; set; }
        [Required]
        public Option Option { get; set; }
        [Required]
        public int PlacedPoints { get; set; }
        
        [Required]
        public DbUser User { get; set; }
        [Required]
        public Bet Bet { get; set; }
    }
}