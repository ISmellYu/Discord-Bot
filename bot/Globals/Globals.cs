using System;
using dcBot.Helpers;

namespace dcBot
{
    public static class Globals
    {
#if !DEBUG
        public static DataWrapper
            Db = new(
                @"Server=localhost;Database=discord;Uid=bot;Pwd=YsCSh2LC;"); //We must initialize firstly to next acces db
#else
            public static DataWrapper Db =
 new(@"Server=20.52.55.98;Database=discord_debug;Uid=bot_debug;Pwd=F9TdXXzy;");
#endif
        public static int COST_MUTE = 100;
        public static int DAILY_AMOUNT = 100;
        public static int JACKPOT_SECONDS = 30;
        public static int DAILY_HOUR_RESET = 0;
        public static int ROULETTE_SECONDS = 30;
        public static string ROULETTE_CHANNEL_NAME = "ruletka";
        public static string BOT_CHANNEL_NAME = "boty";
        public static string BLACKJACK_CHANNEL_NAME = "blackjack";
        public static string ROLE_NAME = "Zwerfikowany";
        public static bool PrintResponseIfNotRightChannel = false;

        public static ulong ROLE_ID = 596323688341700639;
        //public static ulong ROLE_ID = 713860899626287224;
        
        public static bool ENABLE_JACKPOT = false;

        public static Random RND = new();
    }
}