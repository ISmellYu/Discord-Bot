using dcBot.Run;

namespace dcBot
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            //if (!Globals.Db.CheckDatabaseIntegrity()) Console.WriteLine("Someting went wrong!");
            var bt = new Bot();
            bt.RunAsync().GetAwaiter().GetResult();
        }
    }
}