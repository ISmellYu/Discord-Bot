using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace bot.Models
{
    public partial class DbUser
    {
        public void AddPoints(int amount)
        {
            Points += amount;
        }

        public void RemovePoints(int amount)
        {
            Points -= amount;
        }

        public bool HasEnough(int amount)
        {
            return Points >= amount;
        }

        public void ResetPoints()
        {
            Points = 0;
        }

        public bool IsDailyRedeemed()
        {
            return Daily;
        }

        public void RedeemDaily()
        {
            if (IsDailyRedeemed()) return;
            Points += Globals.DAILY_AMOUNT;
            Daily = true;
        }

        public void ResetDaily()
        {
            Daily = false;
        }

        public int PlaceInRanking()
        {
            using var context = new DiscordContext();
            return context.Users.OrderByDescending(p => p.Points).ToList().FindIndex(p => p.UDiscordId == UDiscordId) + 1;
        }
        
    }

    public static class Extensions
    {
        public static DbUser GetUserByUlong(this DbSet<DbUser> users, ulong id)
        {
            return users.SingleOrDefault(p => p.DiscordId == (long) id);
        }
        
        public static DbUser GetUserByDiscordMember(this DbSet<DbUser> users, DiscordMember member)
        {
            return users.SingleOrDefault(p => p.DiscordId == (long) member.Id);
        }
        
        public static bool CheckIfExists(this DbSet<DbUser> users, DiscordMember member)
        {
            return users.GetUserByDiscordMember(member) != null;
        }

        public static void ResetAllDaily(this DbSet<DbUser> users)
        {
            using var context = new DiscordContext();
            foreach (var user in users)
            {
                user.ResetDaily();
            }

            context.SaveChanges();
        }
    }
}