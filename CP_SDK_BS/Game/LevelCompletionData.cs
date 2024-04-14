using System.Linq;

namespace CP_SDK_BS.Game
{
    /// <summary>
    /// Level completion data
    /// </summary>
    public class LevelCompletionData
    {
        /// <summary>
        /// Level type
        /// </summary>
        public LevelType Type { get; internal set; } = LevelType.None;
        /// <summary>
        /// Level data
        /// </summary>
        public GameplayCoreSceneSetupData Data { get; internal set; }
        /// <summary>
        /// Results
        /// </summary>
        public LevelCompletionResults Results { get; internal set; } = null;
        /// <summary>
        /// Max possible multiplied score
        /// </summary>
        public int MaxMultipliedScore { get; internal set; }
        /// <summary>
        /// Is a noodle extension map?
        /// </summary>
        public bool IsNoodle
        {
            get
            {
#if BEATSABER_1_35_0_OR_NEWER
                if (Levels.TryGetCustomRequirementsFor(Data.beatmapLevel, Data.beatmapKey.beatmapCharacteristic, Data.beatmapKey.difficulty, out var l_Reqs))
                    return l_Reqs.Count(x => x.ToLower() == "Noodle Extensions".ToLower()) != 0;
#else
                var l_ExtraData = SongCore.Collections.RetrieveDifficultyData(Data.difficultyBeatmap);
                if (l_ExtraData != null)
                    return l_ExtraData.additionalDifficultyData._requirements.Count(x => x.ToLower() == "Noodle Extensions".ToLower()) != 0;
#endif

                return false;
            }
        }
        /// <summary>
        /// Is a chroma extension map?
        /// </summary>
        public bool IsChroma
        {
            get
            {
#if BEATSABER_1_35_0_OR_NEWER
                if (Levels.TryGetCustomRequirementsFor(Data.beatmapLevel, Data.beatmapKey.beatmapCharacteristic, Data.beatmapKey.difficulty, out var l_Reqs))
                    return l_Reqs.Count(x => x.ToLower() == "Chroma".ToLower()) != 0;
#else
                var l_ExtraData = SongCore.Collections.RetrieveDifficultyData(Data.difficultyBeatmap);
                if (l_ExtraData != null)
                    return l_ExtraData.additionalDifficultyData._requirements.Count(x => x.ToLower() == "Chroma".ToLower()) != 0;
#endif

                return false;
            }
        }
        /// <summary>
        /// Is a replay
        /// </summary>
        public bool IsReplay { get; internal set; }
    }
}
