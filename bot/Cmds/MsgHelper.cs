using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace dcBot.Cmds
{
    public static class MsgHelper
    {
        public static async Task WrongNumber(CommandContext ctx)
        {
            var user = ctx.Member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Zla liczba",
                Description = $"{emoji} Podaj liczbe dodatnia",
                Color = new DiscordColor(0xFF0000)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed);
        }

        public static async Task NotEnoughPts(CommandContext ctx)
        {
            var user = ctx.Member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Niewystarczajaco punktow",
                Description = $"{emoji} Podaj prawidlowa ilosc punktow",
                Color = new DiscordColor(0xFF0000)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed);
        }

        public static async Task NotEnoughPts(CommandContext ctx, DiscordMember member)
        {
            var user = member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Niewystarczajaco punktow",
                Description = $"{emoji} Podaj prawidlowa ilosc punktow",
                Color = new DiscordColor(0xFF0000)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed);
        }

        public static async Task UserNotFound(CommandContext ctx)
        {
            var user = ctx.Member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Blad",
                Description = $"{emoji} Nie znaleziono uzytkownika",
                Color = new DiscordColor(0xFF0000)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed);
        }

        public static async Task AlreadyMuted(CommandContext ctx)
        {
            var user = ctx.Member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Blad",
                Description = $"{emoji} Podany uzytkownik jest juz wyciszony",
                Color = new DiscordColor(0xFF0000)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed);
        }

        public static async Task NotOnChannel(CommandContext ctx)
        {
            var user = ctx.Member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Blad",
                Description = $"{emoji} Podany uzytkownik nie jest na kanale glosowym",
                Color = new DiscordColor(0xFF0000)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed);
        }

        public static async Task Muting(CommandContext ctx, string mentionAuthor, string mentionMuted, int minutes)
        {
            var emoji = DiscordEmoji.FromName(ctx.Client, ":mute:");

            var rightPlurar = "minut";
            switch (minutes)
            {
                case 1:
                    rightPlurar = "minute";
                    break;

                case 2:
                case 3:
                case 4:
                    rightPlurar = "minuty";
                    break;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = "Uzytkownik wyciszony!",
                Description =
                    $"{emoji} {mentionAuthor} wyciszyl uzytkownika {mentionMuted} na __**{minutes}**__ {rightPlurar}",
                Color = new DiscordColor(0, 255, 0),
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync("", embed: embed);
        }

        public static async Task CantBet(CommandContext ctx)
        {
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Nie mozna dolaczyc!",
                Description = $"{emoji} Ruletka jest juz w trakcie losowania",
                Color = new DiscordColor(0xFF0000)
            };
            await ctx.RespondAsync("", embed: embed);
        }

        public static async Task MuteOff(CommandContext ctx)
        {
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Nie mozna mutowac",
                Description = $"{emoji} Aktualnie mutowanie jest wylaczone",
                Color = new DiscordColor(0xFF0000)
            };
            await ctx.RespondAsync("", embed: embed);
        }

        public static async Task WrongChannel(CommandContext ctx)
        {
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Nie mozna wyciszyc",
                Description = $"{emoji} Podany uzytkownik znajduje sie na kanale AFK",
                Color = new DiscordColor(0xFF0000)
            };
            await ctx.RespondAsync("", embed: embed);
        }

        public static async Task DailyAlreadyClaimed(CommandContext ctx)
        {
            var user = ctx.Member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Nie mozna odebrac daily!",
                Description = $"{emoji} Odebrales juz daily tego dnia",
                Color = new DiscordColor(0xFF0000)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        public static async Task DailyClaimed(CommandContext ctx)
        {
            var user = ctx.Member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Odebrano daily!",
                Description = $"{emoji} Punkty daily zostaly wplacone na twoje konto {user.Mention}",
                Color = new DiscordColor(0x7CFC00)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        public static async Task JackpotNotRunning(CommandContext ctx)
        {
            var user = ctx.Member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Jackpot nie istnieje!",
                Description = $"{emoji} Aktualnie zaden jackpot nie jest stworzony",
                Color = new DiscordColor(0xFF0000)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        public static async Task JackpotAlreadyRunning(CommandContext ctx)
        {
            var user = ctx.Member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Jackpot juz istnieje!",
                Description = $"{emoji} Dolacz do obecnego jackpota",
                Color = new DiscordColor(0xFF0000)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        public static async Task RouletteNotRunning(CommandContext ctx)
        {
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Nie ma aktualnie ruletki!",
                Description = $"{emoji} Aktualnie nie ma rouletki, napisz do admina by ja wlaczyl",
                Color = new DiscordColor(0xFF0000)
            };
            await ctx.RespondAsync("", embed: embed);
        }

        public static async Task RouletteRolling(CommandContext ctx)
        {
            var user = ctx.Member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Nie mozesz juz dolaczyc do ruletki!",
                Description = $"{emoji} Ruletka juz losuje, dolacz do nastepnej",
                Color = new DiscordColor(0xFF0000)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        public static async Task RouletteAlreadyRunning(CommandContext ctx)
        {
            var user = ctx.Member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Ruletka juz dziala!",
                Description = $"{emoji} Ruletka juz jest wlaczona",
                Color = new DiscordColor(0xFF0000)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        public static async Task WrongChannelCmd(CommandContext ctx)
        {
            var user = ctx.Member;
            var emoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var embed = new DiscordEmbedBuilder
            {
                Title = "Zly kanal!",
                Description = $"{emoji} Wpisz komende na odpowiednim kanale",
                Color = new DiscordColor(0xFF0000)
            };
            embed.WithAuthor(user.Username, null, user.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        public static bool CheckNumber(int pts)
        {
            return pts >= 1;
        }
    }
}