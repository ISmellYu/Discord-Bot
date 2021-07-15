using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bot.Cmds;
using bot.Helpers;
using bot.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using static bot.Globals;


namespace bot.Games.Jackpot
{
    public class JackpotMain
    {
        public static bool JackpotRunning;
        private static readonly List<DiscordMember> ListMembers = new();
        private static readonly List<string> ListTickets = new();
        private static int _jackpotPool;
        private static bool CanBet;

        private readonly CommandContext ctx;

        public JackpotMain(CommandContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task Brain()
        {
            JackpotRunning = true;
            CanBet = true;

            await StartMessage();
            await Task.Delay(JACKPOT_SECONDS * 1000 - 1000);
            CanBet = false;
            await Task.Delay(1000);

            if (ListMembers.Count == 0)
            {
                await NotEnoughPlayers();
                ResetJackpot();
                return;
            }

            var winner = RollWinner();

            if (winner == null)
            {
                await ctx.RespondAsync("Wystapil blad!");
                return;
            }

            await using (var context = new DiscordContext())
            {
                context.Users.GetUserByDiscordMember(winner).AddPoints(_jackpotPool);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            
            
            //DataWrapper.UsersH.GetUser(winner).AddPoints(_jackpotPool);
            await DisplayWinner(winner);
            ResetJackpot();
        }

        private async Task StartMessage()
        {
            var emoji = DiscordEmoji.FromName(ctx.Client, ":slot_machine:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Jackpot stworzony!",
                Description = $"{emoji} Macie {JACKPOT_SECONDS} sekund aby dolaczyc do jackpota!",
                Color = new DiscordColor(0x7289da)
            };
            await ctx.RespondAsync("", embed: embed.Build());
        }

        private static void ResetJackpot()
        {
            JackpotRunning = false;
            ListMembers.Clear();
            ListTickets.Clear();
            _jackpotPool = 0;
        }

        private static DiscordMember RollWinner()
        {
            ListTickets.Shuffle(RND);
            var stID = ListTickets[RND.Next(0, ListTickets.Count)];
            var rt = ListMembers.SingleOrDefault(p => p.Id.ToString() == stID);
            return rt;
        }

        private async Task DisplayWinner(DiscordMember winner)
        {
            var emoji = DiscordEmoji.FromName(ctx.Client, ":money_with_wings:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Koniec jackpota!",
                Description = $"{emoji} Wygranym jest ||{winner.Mention}|| z pula __**{_jackpotPool}**__",
                Color = new DiscordColor(0xeb9234)
            };
            embed.WithThumbnail(winner.AvatarUrl, 10, 10);
            embed.WithAuthor(winner.Username, null, winner.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        private async Task NotEnoughPlayers()
        {
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Jackpot zatrzymany!",
                Description = $"{emoji} Niewystarczajaco ilosc graczy na jackpocie",
                Color = new DiscordColor(0xFF0000)
            };
            await ctx.RespondAsync("", embed: embed);
        }

        public static async Task JoinJackpot(CommandContext ctx, int pts)
        {
            if (!CanBet)
            {
                await MsgHelper.CantBet(ctx);
                return;
            }

            await using (var context = new DiscordContext())
            {
                context.Users.GetUserByDiscordMember(ctx.Member).RemovePoints(pts);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            
            _jackpotPool += pts;
            var id = ctx.Member.Id.ToString();
            if (!ListMembers.Contains(ctx.Member)) ListMembers.Add(ctx.Member);

            for (var i = 0; i < pts; i++) ListTickets.Add(id);

            await JoinMessage(ctx, pts);

            await DisplayAllPlayers(ctx);
        }

        private static async Task DisplayAllPlayers(CommandContext ctx)
        {
            var lTickets = ListTickets.Count;

            var embed = new DiscordEmbedBuilder
            {
                Title = "Jackpot",
                Color = new DiscordColor(0x7289da)
            };

            foreach (var xMember in ListMembers.OrderByDescending(p => ListTickets.Count(x => p.Id.ToString() == x)))
            {
                var percent = Math.Round((decimal) ListTickets.Count(p => p == xMember.Id.ToString()) / lTickets * 100,
                    2);
                embed.AddField($"{xMember.Username}", $"**{percent,2}%**");
            }

            await ctx.RespondAsync("", embed: embed.Build());
        }

        private static async Task JoinMessage(CommandContext ctx, int pts)
        {
            var emoji = DiscordEmoji.FromName(ctx.Client, ":game_die:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Dolaczyles do jackpota!",
                Description = $"{emoji} {ctx.Member.Mention} dolaczyl do jackopta stawiajac: __**{pts}**__",
                Color = new DiscordColor(0x34eb46)
            };

            await ctx.RespondAsync("", embed: embed);
        }
    }

    public static class Help
    {
        public static void Shuffle<T>(this IList<T> list, Random rnd)
        {
            for (var i = list.Count; i > 0; i--)
                list.Swap(0, rnd.Next(0, i));
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}