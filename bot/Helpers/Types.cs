using Dapper.Contrib.Extensions;
using Newtonsoft.Json;

namespace bot.Helpers
{
    public struct ConfigJson
    {
        [JsonProperty("token")] public string Token { get; private set; }

        [JsonProperty("prefix")] public string CommandPrefix { get; private set; }
    }
}