﻿using Newtonsoft.Json;

namespace CP_SDK_BS.Game.BeatMaps
{
    public class UserDetail
    {
        [JsonProperty] public int id = 0;
        [JsonProperty] public string name = "";
        [JsonProperty] public string hash = "";
        [JsonProperty] public string avatar = "";
    }
}
