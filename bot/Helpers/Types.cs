using Dapper.Contrib.Extensions;
using Newtonsoft.Json;

namespace bot.Helpers
{
    public struct ConfigJson
    {
        [JsonProperty("token")] public string Token { get; private set; }

        [JsonProperty("prefix")] public string CommandPrefix { get; private set; }
    }

    [Table("DbUser")]
    public class DbUser
    {
        public ulong ID { get; }

        public string User { get; }

        public int Amount
        {
            get => DataWrapper.HelpForTypes.GetPts(ID);
            set => DataWrapper.HelpForTypes.SetPts(ID, value);
        }

        public bool Daily
        {
            get => DataWrapper.HelpForTypes.GetDaily(ID);
            set => DataWrapper.HelpForTypes.SetDaily(ID, value);
        }

        public void AddPoints(int pts)
        {
            DataWrapper.HelpForTypes.IncrementPts(ID, pts);
        }

        public void RemovePoints(int pts)
        {
            DataWrapper.HelpForTypes.DecrementPts(ID, pts);
        }

        public void ResetPoints()
        {
            Amount = 0;
        }

        public bool HasEnough(int pts)
        {
            return Amount >= pts;
        }
    }
}