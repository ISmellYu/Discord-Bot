using System;
using System.Threading.Tasks;

namespace bot.Helpers
{
    public static class Functions
    {
        public static async void SafelyExitBot()
        {
            Globals.MUTE_TOKEN.Cancel();
            await Task.WhenAll(Globals.MUTE_TASKS);
            Environment.Exit(1);
        }
    }
}