using System.Collections.Generic;
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
        
        public void RedeemDaily()
        {
            if (IsDailyRedeemed()) return;
            Points += Globals.DAILY_AMOUNT;
            Daily = true;
        }

        public bool IsDailyRedeemed()
        {
            return Daily;
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

        public bool IsAdmin()
        {
            return Globals.AdminsId.Exists(p => p == UDiscordId);
        }
        
    }

    public partial class Bet
    {
        public bool CheckIfValidOption(string option)
        {
            return Options.Contains(option);
        }

        public void WithdrawPrizes(string winOption)
        {
            AllowBetting = false;
            var winners = AllPlacedBets.Where(p => p.Option == winOption);
            foreach (var winner in winners)
            {
                winner.User.AddPoints((int) (winner.PlacedPoints * Odds[winOption]));
            }
        }
    }

    public static class DbSetExtensions
    {
        public static DbUser? GetUserByUlong(this DbSet<DbUser> users, ulong id)
        {
            return users.SingleOrDefault(p => p.DiscordId == (long) id);
        }

        public static List<IndividualBet> GetAllBetsByBetName(this DbSet<IndividualBet> individualBets, string name)
        {
            return individualBets.Where(p => p.Bet.Title == name).ToList();
        }

        public static List<IndividualBet> GetAllBetsByBetOption(this DbSet<IndividualBet> individualBets, string option)
        {
            return individualBets.Where(p => p.Bet.Options.Contains(option)).ToList();
        }
        
        public static bool CheckIfBetExist(this DbSet<Bet> bets, string name)
        {
            return bets.Any(p => p.Title == name);
        }
        
        public static Bet? GetBetByName(this DbSet<Bet> bets, string name)
        {
            return bets.SingleOrDefault(p => p.Title == name);
        }

        public static void AddIndividualBet(this DbSet<IndividualBet> individualBets, string option, int amount,
            DbUser user, Bet bet)
        {
            individualBets.Add(new IndividualBet
            {
                Option = option,
                PlacedPoints = amount,
                Bet = bet,
                User = user
            });
        }

        public static void AddBet(this DbSet<Bet> bets, string name, string firstOption, string secondOption)
        {
            bets.Add(new Bet
            {
                Title = name,
                FirstOption = firstOption,
                SecondOption = secondOption
            });
        }

        public static List<IndividualBet> GetBetsForUser(this DbSet<IndividualBet> bets, DbUser user)
        {
            return bets.Where(p => p.User == user).ToList();
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
        
        public static bool CheckIfExists(this DbSet<DbUser> users, DiscordMember member)
        {
            return users.GetUserByDiscordMember(member) != null;
        }

        public static DbUser? GetUserByDiscordMember(this DbSet<DbUser> users, DiscordMember member)
        {
            return users.SingleOrDefault(p => p.DiscordId == (long) member.Id);
        }

        public static void RemoveUser(this DbSet<DbUser> users, DbUser member)
        {
            users.Remove(member);
        }

        public static void AddMuteUser(this DbSet<MuteUser> muteUsers, DbUser user, int remainingTime)
        {
            muteUsers.Add(new MuteUser()
            {
                RemainingTime = remainingTime,
                User = user
            });
        }
    }
}