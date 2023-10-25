using Newtonsoft.Json;

namespace CP_SDK_BS.Game.BeatMaps
{
    public class MapParitySummary
    {
        [JsonProperty] public int errors = 0;
        [JsonProperty] public int warns = 0;
        [JsonProperty] public int resets = 0;
    }
}
