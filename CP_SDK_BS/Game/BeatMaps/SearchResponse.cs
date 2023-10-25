using Newtonsoft.Json;

namespace CP_SDK_BS.Game.BeatMaps
{
    public class SearchResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public MapDetail[] docs = null;
    }
}
