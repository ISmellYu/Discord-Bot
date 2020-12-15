using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dcBot.Cmds;
using dcBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using static dcBot.Globals;

namespace dcBot.Games.Roulette
{
    public class Roulette
    {
        private static bool _can_bet;
        public static bool _running;
        public static bool _can_run;
        private static readonly List<RoulettePlayer> _ListPlayers = new List<RoulettePlayer>();
        private readonly CommandContext ctx;

        public Roulette(CommandContext ctx)
        {
            this.ctx = ctx;
            _running = true;
            _can_run = true;
        }

        public async Task Main()
        {
            while (_can_run)
            {
                await ShowWelcomeMsg();
                ResetRoulette();
                Thread.Sleep(JACKPOT_SECONDS * 1000 - 1000);
                _can_bet = false;
                Thread.Sleep(1000);
                var clr = RollCollor();
                Task.Run(() => GivePointsToWinners(GetWinners(clr), clr));
                await ShowMessageWin(clr);
            }

            _running = false;
        }

        private static Colors RollCollor()
        {
            var rndnum = RND.Next(0, 14);

            if (rndnum == 0) return Colors.G;

            return rndnum % 2 == 0 ? Colors.B : Colors.R;
        }

        private static void ResetRoulette()
        {
            _can_bet = true;
            _ListPlayers.Clear();
        }

        private async Task ShowMessageWin(Colors clr)
        {
            DiscordEmbedBuilder embed;
            DiscordEmoji emoji;
            switch (clr)
            {
                case Colors.R:
                    emoji = DiscordEmoji.FromName(ctx.Client, ":red_square:");
                    embed = new DiscordEmbedBuilder
                    {
                        Title = "Wygral kolor czerwony!",
                        Description = $"{emoji}",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.RespondAsync("", embed: embed);
                    break;

                case Colors.B:
                    emoji = DiscordEmoji.FromName(ctx.Client, ":black1:");
                    embed = new DiscordEmbedBuilder
                    {
                        Title = "Wygral kolor czarny!",
                        Description = $"{emoji}",
                        Color = new DiscordColor(0x000000)
                    };
                    await ctx.RespondAsync("", embed: embed);
                    break;

                case Colors.G:
                    emoji = DiscordEmoji.FromName(ctx.Client, ":green_square:");
                    embed = new DiscordEmbedBuilder
                    {
                        Title = "Wygral kolor zielony!",
                        Description = $"{emoji}",
                        Color = new DiscordColor(0x008000)
                    };
                    await ctx.RespondAsync("", embed: embed);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(clr), clr, null);
            }
        }

        private async Task ShowWelcomeMsg()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Macie {ROULETTE_SECONDS} sekund na to by postawic punkty!",
                Color = new DiscordColor(0x00ffd6)
            };
            await ctx.RespondAsync("", embed: embed);
        }

        private static IEnumerable<RoulettePlayer> GetWinners(Colors clr)
        {
            return _ListPlayers.Where(p => p.color == clr).ToArray();
        }

        private static async Task GivePointsToWinners(IEnumerable<RoulettePlayer> winners, Colors clr)
        {
            foreach (var player in winners) player.Dbuser.AddPoints(clr == Colors.G ? player.Pts * 14 : player.Pts * 2);
        }

        private static async Task ShowJoinConfirmation(CommandContext ctx, RoulettePlayer user)
        {
            DiscordEmbedBuilder embed;
            DiscordEmoji emoji;
            switch (user.color)
            {
                case Colors.R:
                    emoji = DiscordEmoji.FromName(ctx.Client, ":red_square:");
                    embed = new DiscordEmbedBuilder
                    {
                        Title = $"{user.Member.Username} dolaczyl do ruletki!",
                        Description = $"{user.Member.Mention} postawil na kolor {emoji} {user.Pts} punktow",
                        Color = new DiscordColor(0x800080)
                    };
                    await ctx.RespondAsync("", embed: embed);
                    break;

                case Colors.B:
                    emoji = DiscordEmoji.FromName(ctx.Client, ":black1:");
                    embed = new DiscordEmbedBuilder
                    {
                        Title = $"{user.Member.Username} dolaczyl do ruletki!",
                        Description = $"{user.Member.Mention} postawil na kolor {emoji} {user.Pts} punktow",
                        Color = new DiscordColor(0x800080)
                    };
                    await ctx.RespondAsync("", embed: embed);
                    break;

                case Colors.G:
                    emoji = DiscordEmoji.FromName(ctx.Client, ":green_square:");
                    embed = new DiscordEmbedBuilder
                    {
                        Title = $"{user.Member.Username} dolaczyl do ruletki!",
                        Description = $"{user.Member.Mention} postawil na kolor {emoji} {user.Pts} punktow",
                        Color = new DiscordColor(0x800080)
                    };
                    await ctx.RespondAsync("", embed: embed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static async Task JoinRoulette(CommandContext ctx, char cClr, int pts)
        {
            if (!_can_bet)
            {
                await MsgHelper.RouletteRolling(ctx);
                return;
            }

            var user = new RoulettePlayer(ctx.Member, DataWrapper.UsersH.GetUser(ctx.Member), FromCharToColors(cClr),
                pts);
            user.Dbuser.RemovePoints(pts);
            _ListPlayers.Add(user);
            await ShowJoinConfirmation(ctx, user);
        }

        private static Colors FromCharToColors(char clr)
        {
            return clr switch
            {
                'r' => Colors.R,
                'b' => Colors.B,
                'g' => Colors.G,
                _ => throw new ArgumentException()
            };
        }

        private enum Colors
        {
            R,
            B,
            G
        }

        private class RoulettePlayer
        {
            public readonly Colors color;
            public readonly DbUser Dbuser;
            public readonly DiscordMember Member;
            public readonly int Pts;

            public RoulettePlayer(DiscordMember member, DbUser dbuser, Colors color, int pts)
            {
                Member = member;
                Dbuser = dbuser;
                this.color = color;
                Pts = pts;
            }
        }
    }
}