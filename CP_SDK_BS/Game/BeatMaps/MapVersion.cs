using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CP_SDK_BS.Game.BeatMaps
{
    public enum EMapVersionStates
    {
        UNK,
        Uploaded,
        Testplay,
        Published,
        Feedback
    }

    public class MapVersion
    {
        [JsonProperty] public string hash = "";
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty] public EMapVersionStates state =  EMapVersionStates.UNK;
        [JsonProperty] public string createdAt = "";
        [JsonProperty] public int sageScore = 0;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public MapDifficulty[] diffs = null;
        [JsonProperty] public string downloadURL = "";
        [JsonProperty] public string coverURL = "";
        [JsonProperty] public string previewURL = "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get distinct list of BeatmapCharacteristicSO serialized names in order
        /// </summary>
        /// <returns></returns>
        public List<string> GetBeatmapCharacteristicSOSerializedNamesInOrder()
        {
            List<string> l_Result = new List<string>();

            if (diffs != null)
            {
                l_Result = diffs.Select(x => Levels.SanitizeBeatmapCharacteristicSOSerializedName(x.characteristic))
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .OrderBy(x => Levels.GetBeatmapCharacteristicSOOrdering(x))
                    .ToList();
            }

            return l_Result;
        }
        /// <summary>
        /// Get all difficulties for a specific BeatmapCharacteristicSO
        /// </summary>
        /// <param name="p_BeatmapCharacteristicSerializedName">Target BeatmapCharacteristicSO serialized name</param>
        /// <returns></returns>
        public List<MapDifficulty> GetDifficultiesPerBeatmapCharacteristicSOSerializedName(string p_BeatmapCharacteristicSerializedName)
        {
            List<MapDifficulty> l_Result = new List<MapDifficulty>();

            if (diffs != null)
            {
                var l_BeatmapCharacteristicSOSerializedName = Levels.SanitizeBeatmapCharacteristicSOSerializedName(p_BeatmapCharacteristicSerializedName);
                foreach (var l_Diff in diffs)
                {
                    if (   !Levels.TryGetBeatmapCharacteristicSOBySerializedName(l_Diff.characteristic, out var l_CharacteristicSO)
                        || l_CharacteristicSO.serializedName != l_BeatmapCharacteristicSOSerializedName)
                        continue;

                    l_Result.Add(l_Diff);
                }
            }

            return l_Result;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get cover image bytes
        /// </summary>
        /// <param name="p_Callback">Callback(p_Valid, p_Bytes)</param>
        public void CoverImageBytes(Action<bool, byte[]> p_Callback)
        {
            BeatMapsClient.WebClient.GetAsync(coverURL, CancellationToken.None, (p_Result) =>
            {
                try
                {
                    if (p_Result == null)
                    {
                        p_Callback?.Invoke(false, null);
                        return;
                    }

                    p_Callback?.Invoke(true, p_Result.BodyBytes);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game.BeatMaps][Version.CoverImageBytes] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_Callback?.Invoke(false, null);
                }
            });
        }
        /// <summary>
        /// Get Zip archive bytes
        /// </summary>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback on result</param>
        /// <param name="p_DontRetry">Should not retry in case of failure?</param>
        /// <param name="p_Progress">Progress reporter</param>
        /// <returns></returns>
        public void ZipBytes(CancellationToken p_Token, Action<byte[]> p_Callback, bool p_DontRetry = true, IProgress<float> p_Progress = null)
            => BeatMapsClient.WebClient.DownloadAsync(downloadURL, p_Token, (x) => p_Callback?.Invoke(x?.BodyBytes ?? null), p_DontRetry, p_Progress);
    }
}
