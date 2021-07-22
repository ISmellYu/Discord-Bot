using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace bot.Models
{
    [Index(nameof(DiscordId), IsUnique = true)]
    public partial class DbUser
    {
        public int Id { get; set; }
        
        [Column("DiscordId")]
        [Required]
        public long DiscordId { get; set; }
        
        [NotMapped]
        public ulong UDiscordId => (ulong) DiscordId;

        public int Points { get; set; } = 0;
        public bool Daily { get; set; } = false;
        
        [NotMapped]
        public string? UserName { get; set; }
    }
}