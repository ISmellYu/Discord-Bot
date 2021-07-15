using System;
using bot.Games;
using bot.Helpers;
using DSharpPlus.Entities;

namespace bot
{
    public static class Globals
    {
        public static int COST_MUTE = 100;
        public static int COST_MOVE = 500;
        public static int DAILY_AMOUNT = 100;
        public static int JACKPOT_SECONDS = 30;
        public static int DAILY_HOUR_RESET = 0;
        public static int ROULETTE_SECONDS = 30;
        public static int PTS_PER_MINUTE_INTERVAL_SECONDS = 60;
        public static int PTS_PER_MINUTE_AMOUNT = 3;
        public static int PTS_PER_MINUTE_AMOUNT_BOOSTER = 4;
        public static string ROULETTE_CHANNEL_NAME = "ruletka";
        public static string BOT_CHANNEL_NAME = "boty";
        public static string BLACKJACK_CHANNEL_NAME = "blackjack";
        public static string ROLE_NAME = "Zwerfikowany";
        public static string ROLE_BOOSTER = "Koksy";
        public static bool PrintResponseIfNotRightChannel = false;

#if !DEBUG
        public static ulong ROLE_ID = 596323688341700639;
#else
        public static ulong ROLE_ID = 713860899626287224;
#endif
        
        public static bool ENABLE_JACKPOT = true;
        public static bool ENABLE_MUTE = true;
        public static bool ENABLE_MOVING = false;

        public static Random RND = new();

        public static BetSystem BetsGlobal = new();


    }
}