using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using bot.Models;
using static bot.Globals;

namespace bot.Helpers
{
    public static class Daily
    {
        public static async void DailyThread()
        {
            while (true)
            {
                if (DateTime.Now.Hour == DAILY_HOUR_RESET)
                {
                    await using var context = new DiscordContext();
                    var users = context.Users;
                    users.ResetAllDaily();
                    await Task.Delay(3720000); // 62 minutes
                }
                else
                {
                    await Task.Delay(20000); // 20 seconds
                }
            }
        }
    }
}