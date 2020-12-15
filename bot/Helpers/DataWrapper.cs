using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace dcBot.Helpers
{
    public class DataWrapper
    {
        private static SQLiteConnection _conn;

        public DataWrapper(string path = "db.sqlite")
        {
            _conn = new SQLiteConnection($"Data Source={path}");
            _conn.Open();
            //CheckDatabaseIntegrity();
        }

        public bool CheckDatabaseIntegrity()
        {
            _conn.Execute(
                "CREATE TABLE IF NOT EXISTS discount(ID INT,user TEXT)");
            _conn.Execute(
                "CREATE TABLE IF NOT EXISTS DbUser(ID UNSIGNED BIG INT , user TEXT, amount INTEGER, daily BOOLEAN NOT NULL DEFAULT true)");
            _conn.Execute("CREATE TABLE IF NOT EXISTS roulette(color TEXT, counter INTEGER, UNIQUE(color))");
            _conn.Execute(@"INSERT OR IGNORE INTO roulette(color, counter) VALUES (@clr, @cnt)",
                new[] {new {clr = "red", cnt = 0}, new {clr = "blue", cnt = 0}, new {clr = "green", cnt = 0}});
            _conn.Execute("CREATE TABLE IF NOT EXISTS mutes(ID INT, user TEXT, rem_time INTEGER)");
            return true;
        }

        ~DataWrapper()
        {
            _conn.Close();
        }

        public static class UsersH
        {
            public static DbUser GetUser(DiscordMember mem)
            {
                if (!Exists(mem))
                    CreateUser(mem);
                return _conn.Get<DbUser>(mem.Id);
            }

            public static async Task<DbUser> GetUser(ulong id, CommandContext ctx)
            {
                var mem = await ctx.Guild.GetMemberAsync(id);
                if (!Exists(id))
                    CreateUser(mem);
                return _conn.Get<DbUser>(id);
            }

            public static bool Exists(ulong id)
            {
                return _conn.ExecuteScalar<bool>(@"SELECT count(1) FROM DbUser WHERE ID=@id", new {id});
            }

            public static bool Exists(DiscordMember mem)
            {
                return _conn.ExecuteScalar<bool>(@"SELECT count(1) FROM DbUser WHERE ID=@ID", new {mem.Id});
            }

            public static void CreateUser(ulong id, string name, int amount = 0, bool daily = true)
            {
                _conn.Execute(@"INSERT INTO DbUser VALUES (@ID, @name, @amount, @daily)",
                    new {id, name, amount, daily});
            }

            public static void CreateUser(DiscordMember us)
            {
                CreateUser(us.Id, us.Username);
            }
        }

        public static class HelpForTypes
        {
            public static bool GetDaily(ulong id)
            {
                return _conn.QuerySingle<bool>(@"SELECT daily FROM DbUser WHERE ID=@ID", new {id});
            }

            public static void SetDaily(ulong id, bool state)
            {
                _conn.Execute(@"UPDATE DbUser SET daily=@state WHERE ID=@ID", new {state, id});
            }

            public static int GetPts(ulong id)
            {
                return _conn.QueryFirst<int>(@"SELECT amount FROM DbUser WHERE ID=@ID", new {id});
            }

            public static void SetPts(ulong id, int pts)
            {
                _conn.Execute(@"UPDATE DbUser SET amount=@pts WHERE ID=@ID", new {pts, id});
            }

            public static void IncrementPts(ulong id, int pts)
            {
                _conn.Execute(@"UPDATE DbUser SET amount=amount+@pts WHERE ID=@ID", new {pts, id});
            }

            public static void DecrementPts(ulong id, int pts)
            {
                _conn.Execute(@"UPDATE DbUser SET amount=amount-@pts WHERE ID=@ID", new {pts, id});
            }

            public static DbUser[] GetAllDbUsers()
            {
                return _conn.GetAll<DbUser>().ToArray();
            }

            public static DbUser[] GetTopUsers()
            {
                return GetAllDbUsers().OrderByDescending(p => p.Amount).Take(5).ToArray();
            }
        }
    }
}