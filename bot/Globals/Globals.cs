using System;
using dcBot.Helpers;

namespace dcBot
{
    public static class Globals
    {
        public static DataWrapper Db = new DataWrapper();
        public static int COST_MUTE = 100;
        public static int DAILY_AMOUNT = 100;
        public static int JACKPOT_SECONDS = 30;
        public static int DAILY_HOUR_RESET = 0;
        public static int ROULETTE_SECONDS = 30;
        public static string ROULETTE_CHANNEL_NAME = "ruletka";
        public static string BLACKJACK_CHANNEL_NAME = "blackjack";
        public static string ROLE_NAME = "Zwerfikowany";

        public static ulong ROLE_ID = 596323688341700639;
        //public static ulong ROLE_ID = 713860899626287224;

        public static Random RND = new Random();
    }
}