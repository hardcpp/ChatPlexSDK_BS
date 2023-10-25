﻿using Newtonsoft.Json;

namespace CP_SDK_BS.Game.BeatMaps
{
    public class MapDetailMetadata
    {
        [JsonProperty] public float bpm = 0;
        [JsonProperty] public int duration = 0;
        [JsonProperty] public string songName = "";
        [JsonProperty] public string songSubName = "";
        [JsonProperty] public string songAuthorName = "";
        [JsonProperty] public string levelAuthorName = "";
    }
}
