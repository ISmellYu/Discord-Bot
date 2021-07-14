using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MySqlConnector;

namespace bot.Helpers
{
    public class DataWrapper
    {
        private static MySqlConnection _conn;
        private static string _connectionString;

        public DataWrapper(string connectionString)
        {
            _conn = new MySqlConnection(connectionString);
            _connectionString = connectionString;
            _conn.Open();
            //CheckDatabaseIntegrity();
        }

        // public bool CheckDatabaseIntegrity()
        // {
        //     using var conn = new MySqlConnection(_connectionString);
        //     conn.Execute(
        //         "CREATE TABLE IF NOT EXISTS discount(ID INT,user TEXT)");
        //     conn.Execute(
        //         "CREATE TABLE IF NOT EXISTS DbUser(ID UNSIGNED BIG INT , user TEXT, amount INTEGER, daily BOOLEAN NOT NULL DEFAULT true)");
        //     conn.Execute("CREATE TABLE IF NOT EXISTS roulette(color TEXT, counter INTEGER, UNIQUE(color))");
        //     conn.Execute(@"INSERT OR IGNORE INTO roulette(color, counter) VALUES (@clr, @cnt)",
        //         new[] {new {clr = "red", cnt = 0}, new {clr = "blue", cnt = 0}, new {clr = "green", cnt = 0}});
        //     conn.Execute("CREATE TABLE IF NOT EXISTS mutes(ID INT, user TEXT, rem_time INTEGER)");
        //     return true;
        // }

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
                using var conn = new MySqlConnection(_connectionString);
                return conn.Get<DbUser>(mem.Id);
            }

            public static async Task<DbUser> GetUser(ulong id, CommandContext ctx)
            {
                var mem = await ctx.Guild.GetMemberAsync(id);
                if (!Exists(id))
                    CreateUser(mem);
                using var conn = new MySqlConnection(_connectionString);
                return conn.Get<DbUser>(id);
            }

            public static bool Exists(ulong id)
            {
                using var conn = new MySqlConnection(_connectionString);
                return conn.ExecuteScalar<bool>(@"SELECT count(1) FROM DbUser WHERE ID=@id", new {id});
            }

            public static bool Exists(DiscordMember mem)
            {
                using var conn = new MySqlConnection(_connectionString);
                return conn.ExecuteScalar<bool>(@"SELECT count(1) FROM DbUser WHERE ID=@ID", new {mem.Id});
            }

            public static void CreateUser(ulong id, string name, int amount = 0, bool daily = true)
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Execute(@"INSERT INTO DbUser VALUES (@ID, @name, @amount, @daily)",
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
                using var conn = new MySqlConnection(_connectionString);
                return conn.QuerySingle<bool>(@"SELECT daily FROM DbUser WHERE ID=@ID", new {id});
            }

            public static void SetDaily(ulong id, bool state)
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Execute(@"UPDATE DbUser SET daily=@state WHERE ID=@ID", new {state, id});
            }

            public static int GetPts(ulong id)
            {
                using var conn = new MySqlConnection(_connectionString);
                return conn.QueryFirst<int>(@"SELECT amount FROM DbUser WHERE ID=@ID", new {id});
            }

            public static void SetPts(ulong id, int pts)
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Execute(@"UPDATE DbUser SET amount=@pts WHERE ID=@ID", new {pts, id});
            }

            public static void IncrementPts(ulong id, int pts)
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Execute(@"UPDATE DbUser SET amount=amount+@pts WHERE ID=@ID", new {pts, id});
            }

            public static void DecrementPts(ulong id, int pts)
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Execute("UPDATE DbUser SET amount=amount-@pts WHERE ID=@ID", new {pts, id});
            }

            public static DbUser[] GetAllDbUsers()
            {
                using var conn = new MySqlConnection(_connectionString);
                return conn.GetAll<DbUser>().ToArray();
            }

            public static DbUser[] GetTopUsers()
            {
                using var conn = new MySqlConnection(_connectionString);
                return GetAllDbUsers().OrderByDescending(p => p.Amount).Take(5).ToArray();
            }
            
            public static int GetPlaceForUser(DiscordMember user)
            {
                var totalWatch = System.Diagnostics.Stopwatch.StartNew();
                
                var watch = System.Diagnostics.Stopwatch.StartNew();
                using var conn = new MySqlConnection(_connectionString);
                watch.Stop();
                Console.WriteLine($"Opening database connection takes {watch.ElapsedMilliseconds} ms");

                var watch1 = System.Diagnostics.Stopwatch.StartNew();
                var users = GetAllDbUsers();
                watch1.Stop();
                Console.WriteLine($"Getting all users takes {watch1.ElapsedMilliseconds} ms");
                
                var watch2 = System.Diagnostics.Stopwatch.StartNew();
                var ordered = users.OrderByDescending(p => p.Amount);
                watch2.Stop();
                Console.WriteLine($"Ordering takes {watch2.ElapsedMilliseconds} ms");
                
                var watch3 = System.Diagnostics.Stopwatch.StartNew();
                var listed = ordered.ToList();
                watch3.Stop();
                Console.WriteLine($"Converting to list takes {watch3.ElapsedMilliseconds} ms");
                
                var watch4 = System.Diagnostics.Stopwatch.StartNew();
                var index = listed.FindIndex(p => p.ID == user.Id) + 1;
                watch4.Stop();
                Console.WriteLine($"Finding index takes {watch4.ElapsedMilliseconds} ms");
                
                totalWatch.Stop();
                Console.WriteLine($"Whole function takes {totalWatch.ElapsedMilliseconds} ms");
                return index;
            }
        }

        public static class HelpForBets
        {
            
        }
    }
}