using BeatmapSaveDataCommon;
using BeatmapSaveDataVersion4;
using System.Linq;

namespace CP_SDK_BS.Game
{
    /// <summary>
    /// Level data instance
    /// </summary>
    public class LevelData
    {
        /// <summary>
        /// Level type
        /// </summary>
        public LevelType Type { get; internal set; }
        /// <summary>
        /// Level data
        /// </summary>
        public GameplayCoreSceneSetupData Data { get; internal set; }
        /// <summary>
        /// Max possible multiplied score
        /// </summary>
        public int MaxMultipliedScore { get; internal set; }

        /// <summary>
        /// Level has rotations events
        /// </summary>
        public bool HasRotations
        {
            get
            {
                var l_SpawnRotationEventsCount = Data?.transformedBeatmapData?.allBeatmapDataItems.Count(
                    x => x.type == BeatmapDataItem.BeatmapDataItemType.BeatmapObject
                        && (x as BeatmapObjectData).rotation != 0
                );

                return l_SpawnRotationEventsCount > 0;
            }
        }

        /// <summary>
        /// Is a noodle extension map?
        /// </summary>
        public bool IsNoodle
        {
            get
            {
                if (Levels.TryGetCustomRequirementsFor(Data.beatmapLevel, Data.beatmapKey.beatmapCharacteristic, Data.beatmapKey.difficulty, out var l_Reqs))
                    return l_Reqs.Count(x => x.ToLower() == "Noodle Extensions".ToLower()) != 0;

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
                if (Levels.TryGetCustomRequirementsFor(Data.beatmapLevel, Data.beatmapKey.beatmapCharacteristic, Data.beatmapKey.difficulty, out var l_Reqs))
                    return l_Reqs.Count(x => x.ToLower() == "Chroma".ToLower()) != 0;

                return false;
            }
        }
        /// <summary>
        /// Is a replay
        /// </summary>
        public bool IsReplay { get; internal set; }
    }
}
