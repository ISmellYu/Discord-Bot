using System;
using System.Linq;
using bot.Models;
using bot.Run;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace bot
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using (var context = new DiscordContext())
            {
                context.Database.EnsureCreated();
            }
            //if (!Globals.Db.CheckDatabaseIntegrity()) Console.WriteLine("Someting went wrong!");
            var bt = new Bot();
            bt.RunAsync().GetAwaiter().GetResult();
        }
    }
}