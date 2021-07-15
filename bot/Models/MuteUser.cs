using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace bot.Models
{
    public class MuteUser
    {
        public int Id { get; set; }
        [Required]
        public int RemainingTime { get; set; }
        
        [Required]
        public DbUser User { get; set; }
    }
}