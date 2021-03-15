using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dcBot.Cmds;
using dcBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using NetDeck.Cards;
using NetDeck.Decks;

namespace dcBot.Games.BlackJack
{
    public class BlackJack
    {
        public static bool _isRunning;
        private readonly int _amount;
        private readonly List<StandardPlayingCard> _authorDeck = new();
        private readonly StandardDeck _deck;
        private readonly DiscordEmoji _emojiHit;
        private readonly DiscordEmoji _emojiNO;
        private readonly DiscordEmoji _emojiOK;
        private readonly DiscordEmoji _emojiStand;
        private readonly DiscordMember _opponent;
        private readonly List<StandardPlayingCard> _opponentDeck = new();
        private int _stands;
        private string _turn = "p1";
        private readonly CommandContext ctx;


        public BlackJack(CommandContext ctx, DiscordMember opponent, int amount)
        {
            _deck = new StandardDeck();
            this.ctx = ctx;
            _emojiOK = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            _emojiNO = DiscordEmoji.FromName(ctx.Client, ":x:");
            _emojiStand = DiscordEmoji.FromName(ctx.Client, ":regional_indicator_s:");
            _emojiHit = DiscordEmoji.FromName(ctx.Client, ":regional_indicator_h:");
            _amount = amount;
            _opponent = opponent;
        }

        public async Task Brain()
        {
            _isRunning = true;
            var rtV = await SendMsgReact();

            if (!rtV)
            {
                _isRunning = false;
                return;
            }

            await GameLoop();

            _isRunning = false;
        }

        private async Task GameLoop()
        {
            await DrawStarterCards();

            if (await CheckIfWin())
            {
                await EndBoard();
                await WinMessage();
                return;
            }

            if (_authorDeck.Sum(p => p.Value) > 21)
            {
                _turn = "p2";
                await EndBoard();
                await WinMessage();
                return;
            }


            _turn = "p2";
            if (await CheckIfWin())
            {
                await EndBoard();
                await WinMessage();
                return;
            }

            if (_opponentDeck.Sum(p => p.Value) > 21)
            {
                _turn = "p1";
                await EndBoard();
                await WinMessage();
                return;
            }

            _turn = Globals.RND.Next(2) == 0 ? "p1" : "p2";
            while (true)
                switch (_turn)
                {
                    case "p1":
                    {
                        while (_turn == "p1")
                        {
                            var isStanding = await PrintBoardAndGetReaction();
                            if (isStanding)
                            {
                                _stands++;
                                if (_stands == 2)
                                {
                                    var sA = _authorDeck.Sum(p => p.Value);
                                    var sO = _opponentDeck.Sum(p => p.Value);
                                    if (sA > sO)
                                    {
                                        await EndBoard();
                                        await WinMessage();
                                        return;
                                    }

                                    if (sA < sO)
                                    {
                                        _turn = "p2";
                                        await EndBoard();
                                        await WinMessage();
                                        return;
                                    }

                                    await EndBoard();
                                    await Draw();
                                    return;
                                }

                                _turn = "p2";
                            }
                            else
                            {
                                _stands = 0;
                                var card = _deck.DrawCard();

                                if (card.Rank == "A") card.Value = await AskForA();

                                _authorDeck.Add(card);

                                if (await CheckIfWin())
                                {
                                    await EndBoard();
                                    await WinMessage();
                                    return;
                                }

                                if (_authorDeck.Sum(p => p.Value) <= 21) continue;
                                _turn = "p2";
                                await EndBoard();
                                await WinMessage();
                                return;
                            }
                        }

                        break;
                    }

                    case "p2":
                    {
                        while (_turn == "p2")
                        {
                            var isStanding = await PrintBoardAndGetReaction();
                            if (isStanding)
                            {
                                _stands++;
                                if (_stands == 2)
                                {
                                    var sA = _authorDeck.Sum(p => p.Value);
                                    var sO = _opponentDeck.Sum(p => p.Value);
                                    if (sA > sO)
                                    {
                                        _turn = "p1";
                                        await EndBoard();
                                        await WinMessage();
                                        return;
                                    }

                                    if (sA < sO)
                                    {
                                        _turn = "p2";
                                        await EndBoard();
                                        await WinMessage();
                                        return;
                                    }

                                    await EndBoard();
                                    await Draw();
                                    return;
                                }

                                _turn = "p1";
                            }
                            else
                            {
                                _stands = 0;
                                var card = _deck.DrawCard();

                                if (card.Rank == "A") card.Value = await AskForA();

                                _opponentDeck.Add(card);

                                if (await CheckIfWin())
                                {
                                    await EndBoard();
                                    await WinMessage();
                                    return;
                                }

                                if (_opponentDeck.Sum(p => p.Value) <= 21) continue;
                                _turn = "p1";
                                await EndBoard();
                                await WinMessage();
                                return;
                            }
                        }

                        break;
                    }
                }
        }

        private async Task DrawStarterCards()
        {
            for (var i = 0; i <= 1; i++)
            {
                _turn = "p1";
                var card = _deck.DrawCard();
                if (card.Rank == "A")
                {
                    var vl = await AskForA();
                    card.Value = vl;
                }

                _authorDeck.Add(card);

                _turn = "p2";
                card = _deck.DrawCard();
                if (card.Rank == "A")
                {
                    var vl = await AskForA();
                    card.Value = vl;
                }

                _opponentDeck.Add(card);
            }

            _turn = "p1";
        }

        private async Task<bool> SendMsgReact()
        {
            var DbAuthor = DataWrapper.UsersH.GetUser(ctx.Member);
            var msg = await StartMessage();

            var interactivity = ctx.Client.GetInteractivity();

            try
            {
                var reactionResult = await interactivity.WaitForReactionAsync(
                    p => p.Emoji == _emojiNO || p.Emoji == _emojiOK, msg,
                    _opponent, TimeSpan.FromSeconds(30));

                if (reactionResult.Result.Emoji == _emojiNO)
                {
                    await OpponentDeclined();
                    DbAuthor.AddPoints(_amount);
                    return false;
                }
            }
            catch (NullReferenceException)
            {
                await OpponentNotJoined();
                DbAuthor.AddPoints(_amount);
                return false;
            }

            var DbOpponent = DataWrapper.UsersH.GetUser(_opponent);
            if (!DbOpponent.HasEnough(_amount))
            {
                await MsgHelper.NotEnoughPts(ctx, _opponent);
                DbAuthor.AddPoints(_amount);
                return false;
            }

            await OpponentAccepted();
            DbOpponent.RemovePoints(_amount);
            return true;
        }

        private async Task<DiscordMessage> StartMessage()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Blackjack stworzony!",
                Description =
                    $"{DiscordEmoji.FromName(ctx.Client, ":black_joker:")} {_opponent.Mention} zostal zaproszony do gry w blackjacka, aby dolaczyc wybierz emotke {_emojiOK}, jezeli nie chcesz wybierz {_emojiNO}",
                Color = new DiscordColor(0, 255, 0)
            };
            embed.WithAuthor(ctx.Member.Username, null, ctx.Member.AvatarUrl);
            var msg = await ctx.RespondAsync("", embed: embed.Build());

            await msg.CreateReactionAsync(_emojiOK);
            await msg.CreateReactionAsync(_emojiNO);

            return msg;
        }

        private async Task Draw()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Remis!",
                Description = "Nikt nie wygral",
                Color = new DiscordColor(0, 255, 0)
            };
            await ctx.RespondAsync("", embed: embed.Build());

            DataWrapper.UsersH.GetUser(ctx.Member).AddPoints(_amount);
            DataWrapper.UsersH.GetUser(_opponent).AddPoints(_amount);
        }

        private async Task WinMessage()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Wygral {(_turn == "p1" ? ctx.User.Username : _opponent.Username)}",
                Description =
                    $"{DiscordEmoji.FromName(ctx.Client, ":tada:")} Wygranym jest: {(_turn == "p1" ? ctx.User.Mention : _opponent.Mention)} {DiscordEmoji.FromName(ctx.Client, ":tada:")}!",
                Color = new DiscordColor(0, 255, 0)
            };
            embed.WithAuthor(_turn == "p1" ? ctx.User.Username : _opponent.Username, null,
                _turn == "p1" ? ctx.User.AvatarUrl : _opponent.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());

            DataWrapper.UsersH.GetUser(_turn == "p1" ? ctx.Member : _opponent).AddPoints(_amount * 2);
        }

        private async Task OpponentNotJoined()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Uzytkownik nie dolaczyl",
                Description = $"{_emojiNO} {_opponent.Mention} nie dolaczyl na czas do blackjacka!",
                Color = new DiscordColor(255, 0, 0)
            };
            embed.WithAuthor(ctx.Member.Username, null, ctx.Member.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        private async Task OpponentDeclined()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Uzytkownik odrzucil zaproszenie",
                Description =
                    $"{_emojiNO} {_opponent.Mention} nie dolaczyl na czas do blackjacka gracza {ctx.Member.Mention}!",
                Color = new DiscordColor(255, 0, 0)
            };
            embed.WithAuthor(ctx.Member.Username, null, ctx.Member.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        private async Task OpponentAccepted()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Uzytkownik zaakceptowal zaproszenie",
                Description = $"{_emojiOK} {_opponent.Mention} dolaczyl do blackjacka gracza {ctx.Member.Mention}!",
                Color = new DiscordColor(0, 255, 0)
            };
            embed.WithAuthor(_opponent.Username, null, _opponent.AvatarUrl);
            await ctx.RespondAsync("", embed: embed.Build());
        }

        private async Task<bool> PrintBoardAndGetReaction()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Tura {(_turn == "p1" ? ctx.Member.Username : _opponent.Username)}",
                Description =
                    $"Karty {ctx.Member.Mention} **{string.Join(" | ", _authorDeck.Select(p => $"{p.Rank} Of {p.FullSuitName}"))}** __Suma: {_authorDeck.Sum(p => p.Value)}__\n\nKarty {_opponent.Mention} **{string.Join(" | ", _opponentDeck.Select(p => $"{p.Rank} Of {p.FullSuitName}"))}** __Suma: {_opponentDeck.Sum(p => p.Value)}__",
                Color = _turn == "p1" ? new DiscordColor(0, 220, 255) : new DiscordColor(255, 0, 73)
            };
            embed.WithAuthor(_turn == "p1" ? ctx.Member.Username : _opponent.Username, null,
                _turn == "p1" ? ctx.Member.AvatarUrl : _opponent.AvatarUrl);
            var msg = await ctx.RespondAsync("", embed: embed.Build());

            await msg.CreateReactionAsync(_emojiStand);
            await msg.CreateReactionAsync(_emojiHit);

            var interactivity = ctx.Client.GetInteractivity();

            try
            {
                var reactionResult = await interactivity.WaitForReactionAsync(
                    p => p.Emoji == _emojiStand || p.Emoji == _emojiHit, msg,
                    _turn == "p1" ? ctx.Member : _opponent, TimeSpan.FromSeconds(30));

                return reactionResult.Result.Emoji == _emojiStand;
            }
            catch (NullReferenceException)
            {
                return true;
            }
        }

        private async Task<int> AskForA()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Wybierz liczbe",
                Description =
                    $"{DiscordEmoji.FromName(ctx.Client, ":black_joker:")} {(_turn == "p1" ? ctx.Member.Mention : _opponent.Mention)} wybierz bardziej pasujaca dla ciebie liczbe.\nTwoje karty to: **{(_turn == "p1" ? string.Join(" | ", _authorDeck.Select(p => $"{p.Rank} Of {p.FullSuitName}")) : string.Join(" | ", _opponentDeck.Select(p => $"{p.Rank} Of {p.FullSuitName}")))}** ",
                Color = new DiscordColor(0, 255, 0)
            };
            embed.WithAuthor(_turn == "p1" ? ctx.Member.Username : _opponent.Username, null,
                _turn == "p1" ? ctx.Member.AvatarUrl : _opponent.AvatarUrl);
            var msg = await ctx.RespondAsync("", embed: embed.Build());

            var oneEmoji = DiscordEmoji.FromName(ctx.Client, ":one:");
            var elevenEmoji = DiscordEmoji.FromName(ctx.Client, ":eleven:");

            await msg.CreateReactionAsync(oneEmoji);
            await msg.CreateReactionAsync(elevenEmoji);

            var interactivity = ctx.Client.GetInteractivity();

            try
            {
                var reactionResult = await interactivity.WaitForReactionAsync(
                    p => p.Emoji == oneEmoji || p.Emoji == elevenEmoji, msg,
                    _turn == "p1" ? ctx.Member : _opponent, TimeSpan.FromSeconds(30));

                return reactionResult.Result.Emoji == oneEmoji ? 1 : 11;
            }
            catch (NullReferenceException)
            {
                return 1;
            }
        }

        private async Task<bool> CheckIfWin()
        {
            if (_turn == "p1")
            {
                var x = _authorDeck.Sum(p => p.Value);
                if (x == 21) return true;
            }
            else
            {
                var x = _opponentDeck.Sum(p => p.Value);
                if (x == 21) return true;
            }

            return false;
        }

        private async Task EndBoard()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Koniec",
                Description =
                    $"Karty {ctx.Member.Mention} **{string.Join(" | ", _authorDeck.Select(p => $"{p.Rank} Of {p.FullSuitName}"))}** __Suma: {_authorDeck.Sum(p => p.Value)}__\n\nKarty {_opponent.Mention} **{string.Join(" | ", _opponentDeck.Select(p => $"{p.Rank} Of {p.FullSuitName}"))}** __Suma: {_opponentDeck.Sum(p => p.Value)}__",
                Color = new DiscordColor(0, 255, 0)
            };
            await ctx.RespondAsync("", embed: embed.Build());
        }
    }
}