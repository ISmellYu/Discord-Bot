using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static bot.Globals;

namespace bot.Helpers
{
    public static class Daily
    {
        public static async Task DailyThread()
        {
            while (true)
            {
                if (DateTime.Now.Hour == DAILY_HOUR_RESET)
                {
                    var users = DataWrapper.HelpForTypes.GetAllDbUsers();
                    ResetAllDaily(users);
                    await Task.Delay(3720000); // 62 minutes
                }
                else
                {
                    await Task.Delay(20000); // 20 seconds
                }
            }
        }

        private static void ResetAllDaily(IEnumerable<DbUser> users)
        {
            foreach (var x in users) x.Daily = true;
        }
    }
}