using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CP_SDK_BS.Game
{
    /// <summary>
    /// Level helper
    /// </summary>
    public class Levels
    {
        private static List<Action>                         m_ReloadSongsCallbacks = new List<Action>(10);

#if BEATSABER_1_35_0_OR_NEWER
        private static BeatmapCharacteristicCollection      m_BeatmapCharacteristicCollection;
#else
        private static IAdditionalContentModel              m_AdditionalContentModel;
#endif
        private static BeatmapLevelsModel                   m_BeatmapLevelsModel;
        private static CancellationTokenSource              m_GetLevelCancellationTokenSource;
        private static CancellationTokenSource              m_GetLevelEntitlementStatusTokenSource;
        private static MenuTransitionsHelper                m_MenuTransitionsHelper;
#if BEATSABER_1_35_0_OR_NEWER
        private static SimpleLevelStarter                   m_SimpleLevelStarter;
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get default pack cover
        /// </summary>
        /// <returns></returns>
        public static Sprite GetDefaultPackCover()
            => SongCore.Loader.defaultCoverImage;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reload songs
        /// </summary>
        /// <param name="p_Full">Full reload?</param>
        /// <param name="p_Callback">On finish callback</param>
        public static void ReloadSongs(bool p_Full, Action p_Callback = null)
        {
            SongCore.Loader.SongsLoadedEvent -= ReloadSongs_Callback;
            SongCore.Loader.SongsLoadedEvent += ReloadSongs_Callback;

            if (p_Callback != null)
            {
                lock (m_ReloadSongsCallbacks)
                    m_ReloadSongsCallbacks.Add(p_Callback);
            }

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => SongCore.Loader.Instance.RefreshSongs(p_Full));
        }
        /// <summary>
        /// Reload songs callback
        /// </summary>
#if BEATSABER_1_35_0_OR_NEWER
        private static void ReloadSongs_Callback(SongCore.Loader _, ConcurrentDictionary<string, BeatmapLevel> __)
#else
        private static void ReloadSongs_Callback(SongCore.Loader _, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> __)
#endif
        {
            SongCore.Loader.SongsLoadedEvent -= ReloadSongs_Callback;

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                var l_Callbacks = null as List<Action>;
                lock (m_ReloadSongsCallbacks)
                {
                    l_Callbacks = new List<Action>(m_ReloadSongsCallbacks);
                    m_ReloadSongsCallbacks.Clear();
                }

                foreach (var l_Current in l_Callbacks)
                    l_Current();
            });
        }
        /// <summary>
        /// Check for mapping capability
        /// </summary>
        /// <param name="p_Capability">Capability name</param>
        /// <returns>True or false</returns>
        public static bool HasMappingCapability(string p_Capability)
        {
            return SongCore.Collections.capabilities.Contains(p_Capability);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sanitize a level ID for case matching
        /// </summary>
        /// <param name="p_LevelID">Input level ID</param>
        /// <returns>Sanitized level ID</returns>
        public static string SanitizeLevelID(string p_LevelID)
        {
            if (TryGetHashFromLevelID(p_LevelID, out var l_LevelHash))
            {
                /// Level hash is sanitized by TryGetHashFromLevelID
                return $"custom_level_{l_LevelHash}";
            }

            return p_LevelID;
        }
        /// <summary>
        /// Try get hash from level ID
        /// </summary>
        /// <param name="p_LevelID">Input level ID</param>
        /// <param name="p_Hash">OUT hash</param>
        /// <returns>true or false</returns>
        public static bool TryGetHashFromLevelID(string p_LevelID, out string p_Hash)
        {
            p_Hash = string.Empty;
            if (!LevelID_IsCustom(p_LevelID))
                return false;

            p_Hash = p_LevelID.Substring(13);
            if (p_Hash.Length == 40/* TODO check for only hex*/)
                p_Hash = p_Hash.ToUpper();

            return true;
        }
        /// <summary>
        /// Try get level ID from hash
        /// </summary>
        /// <param name="p_Hash">Input hash</param>
        /// <param name="p_LevelID">OUT level ID</param>
        /// <returns>true or false</returns>
        public static bool TryGetLevelIDFromHash(string p_Hash, out string p_LevelID)
        {
            p_LevelID = string.Empty;
            if (string.IsNullOrEmpty(p_Hash) || p_Hash.Length != 40 /* TODO check for only hex*/)
                return false;

            p_LevelID = "custom_level_" + p_Hash.ToUpper();
            return true;
        }
        /// <summary>
        /// Is level ID a custom level ID
        /// </summary>
        /// <param name="p_LevelID">Input level ID</param>
        /// <returns>true or false</returns>
        public static bool LevelID_IsCustom(string p_LevelID)
        {
            if (p_LevelID == null || p_LevelID.Length < 13)
                return false;

            return p_LevelID.StartsWith("custom_level_", StringComparison.OrdinalIgnoreCase);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try get BeatmapCharacteristicSO by serialized name
        /// </summary>
        /// <param name="p_SerializedName">Characteristic serialized name</param>
        /// <param name="p_BeatmapCharacteristicSO">OUT BeatmapCharacteristicSO</param>
        /// <returns>true or false</returns>
        public static bool TryGetBeatmapCharacteristicSOBySerializedName(string p_SerializedName, out BeatmapCharacteristicSO p_BeatmapCharacteristicSO)
        {
            p_BeatmapCharacteristicSO = null;

            var l_SanatizedSerializedName = SanitizeBeatmapCharacteristicSOSerializedName(p_SerializedName);
            var l_Result                  = null as BeatmapCharacteristicSO;

#if BEATSABER_1_35_0_OR_NEWER
            if (m_BeatmapCharacteristicCollection == null)
            {
                var l_CustomLevelLoader = Resources.FindObjectsOfTypeAll<CustomLevelLoader>().FirstOrDefault();
                m_BeatmapCharacteristicCollection = l_CustomLevelLoader?._beatmapCharacteristicCollection;
            }

            if (m_BeatmapCharacteristicCollection == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][Levels.TryGetBeatmapCharacteristicSOBySerializedName] Invalid BeatmapCharacteristicCollection");
                return false;
            }

            l_Result = m_BeatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(l_SanatizedSerializedName);
#else
            l_Result = SongCore.Loader.beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(l_SanatizedSerializedName);
#endif

            if (l_Result != null)
                p_BeatmapCharacteristicSO = l_Result;

            return l_Result != null;
        }
        /// <summary>
        /// Sanitize BeatmapCharacteristicSO serialized name
        /// </summary>
        /// <param name="p_SerializedName">Input serialized name</param>
        /// <returns>Sanatized BeatmapCharacteristicSO serialized name or input</returns>
        public static string SanitizeBeatmapCharacteristicSOSerializedName(string p_SerializedName)
        {
            if (        p_SerializedName.Equals("Standard",     StringComparison.OrdinalIgnoreCase))
                return "Standard";
            else if (   p_SerializedName.Equals("One Saber",    StringComparison.OrdinalIgnoreCase)
                     || p_SerializedName.Equals("OneSaber",     StringComparison.OrdinalIgnoreCase))
                return "OneSaber";
            else if (   p_SerializedName.Equals("No Arrows",    StringComparison.OrdinalIgnoreCase)
                     || p_SerializedName.Equals("NoArrows",     StringComparison.OrdinalIgnoreCase))
                return "NoArrows";
            else if (   p_SerializedName.Equals("360Degree",    StringComparison.OrdinalIgnoreCase))
                return "360Degree";
            else if (   p_SerializedName.Equals("Lawless",      StringComparison.OrdinalIgnoreCase))
                return "Lawless";
            else if (   p_SerializedName.Equals("90Degree",     StringComparison.OrdinalIgnoreCase))
                return "90Degree";
            else if (   p_SerializedName.Equals("LightShow",    StringComparison.OrdinalIgnoreCase)
                     || p_SerializedName.Equals("Lightshow",    StringComparison.OrdinalIgnoreCase))
                return "Lightshow";

            return p_SerializedName;
        }
        /// <summary>
        /// Get ordering value for a BeatmapCharacteristicSO
        /// </summary>
        /// <param name="p_CharacteristicName">Characteristic serialized name</param>
        /// <returns>Sorting order or 1000</returns>
        public static int GetBeatmapCharacteristicSOOrdering(string p_CharacteristicName)
        {
            var l_SerializedName = SanitizeBeatmapCharacteristicSOSerializedName(p_CharacteristicName);
            if (TryGetBeatmapCharacteristicSOBySerializedName(l_SerializedName, out var l_CharacteristicSO))
                return l_CharacteristicSO.sortingOrder;

            return 1000;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BeatmapDifficulty serialized name to difficulty name
        /// </summary>
        /// <param name="p_BeatmapDifficultySerializedName">BeatmapDifficulty serialized name</param>
        /// <returns>Difficulty name</returns>
        public static string BeatmapDifficultySerializedNameToDifficultyName(string p_BeatmapDifficultySerializedName)
        {
            if (p_BeatmapDifficultySerializedName.Equals("easy", StringComparison.OrdinalIgnoreCase))
                return "Easy";
            else if (p_BeatmapDifficultySerializedName.Equals("normal", StringComparison.OrdinalIgnoreCase))
                return "Normal";
            else if (p_BeatmapDifficultySerializedName.Equals("hard", StringComparison.OrdinalIgnoreCase))
                return "Hard";
            else if (p_BeatmapDifficultySerializedName.Equals("expert", StringComparison.OrdinalIgnoreCase))
                return "Expert";
            else if (p_BeatmapDifficultySerializedName.Equals("expertplus", StringComparison.OrdinalIgnoreCase))
                return "Expert+";

            CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.Game][Level.BeatmapDifficultySerializedNameToDifficultyName] Unknown serialized difficulty \"{p_BeatmapDifficultySerializedName}\", fall back to \"? Expert+ ?\"");

            return "? Expert+ ?";
        }
        /// <summary>
        /// BeatmapDifficulty serialized name to difficulty name short
        /// </summary>
        /// <param name="p_BeatmapDifficultySerializedName">BeatmapDifficulty serialized name</param>
        /// <returns>Difficulty name short</returns>
        public static string BeatmapDifficultySerializedNameToDifficultyNameShort(string p_BeatmapDifficultySerializedName)
        {
            if (p_BeatmapDifficultySerializedName.Equals("easy", StringComparison.OrdinalIgnoreCase))
                return "E";
            else if (p_BeatmapDifficultySerializedName.Equals("normal", StringComparison.OrdinalIgnoreCase))
                return "N";
            else if (p_BeatmapDifficultySerializedName.Equals("hard", StringComparison.OrdinalIgnoreCase))
                return "H";
            else if (p_BeatmapDifficultySerializedName.Equals("expert", StringComparison.OrdinalIgnoreCase))
                return "Ex";
            else if (p_BeatmapDifficultySerializedName.Equals("expertplus", StringComparison.OrdinalIgnoreCase))
                return "Ex+";

            CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.Game][Level.BeatmapDifficultySerializedNameToDifficultyNameShort] Unknown serialized difficulty \"{p_BeatmapDifficultySerializedName}\", fall back to ExpertPlus");

            return "E+";
        }
        /// <summary>
        /// BeatmapDifficulty serialized name to BeatmapDifficulty
        /// </summary>
        /// <param name="p_BeatmapDifficultySerializedName">BeatmapDifficulty serialized name</param>
        /// <returns>BeatmapDifficulty</returns>
        public static BeatmapDifficulty BeatmapDifficultySerializedNameToBeatmapDifficulty(string p_BeatmapDifficultySerializedName)
        {
            if (p_BeatmapDifficultySerializedName.Equals("easy", StringComparison.OrdinalIgnoreCase))
                return BeatmapDifficulty.Easy;
            else if (p_BeatmapDifficultySerializedName.Equals("normal", StringComparison.OrdinalIgnoreCase))
                return BeatmapDifficulty.Normal;
            else if (p_BeatmapDifficultySerializedName.Equals("hard", StringComparison.OrdinalIgnoreCase))
                return BeatmapDifficulty.Hard;
            else if (p_BeatmapDifficultySerializedName.Equals("expert", StringComparison.OrdinalIgnoreCase))
                return BeatmapDifficulty.Expert;
            else if (p_BeatmapDifficultySerializedName.Equals("expertplus", StringComparison.OrdinalIgnoreCase))
                return BeatmapDifficulty.ExpertPlus;

            CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.Game][Level.BeatmapDifficultySerializedNameToBeatmapDifficulty] Unknown serialized difficulty \"{p_BeatmapDifficultySerializedName}\", fall back to ExpertPlus");

            return BeatmapDifficulty.ExpertPlus;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Own a DLC level by level ID
        /// </summary>
        /// <param name="p_LevelID">Level ID</param>
        /// <returns></returns>
        public static async Task<bool> OwnDLCLevelByLevelID(string p_LevelID)
        {
            if (LevelID_IsCustom(p_LevelID))
                return true;

#if BEATSABER_1_35_0_OR_NEWER
            if (m_BeatmapLevelsModel == null)
                m_BeatmapLevelsModel = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().FirstOrDefault(x => x._beatmapLevelsModel != null)?._beatmapLevelsModel;

            if (m_BeatmapLevelsModel != null && m_BeatmapLevelsModel._entitlements != null)
            {
                m_GetLevelEntitlementStatusTokenSource?.Cancel();
                m_GetLevelEntitlementStatusTokenSource = new CancellationTokenSource();

                var l_Token = m_GetLevelEntitlementStatusTokenSource.Token;
                return await m_BeatmapLevelsModel._entitlements.GetLevelEntitlementStatusAsync(p_LevelID, l_Token) == EntitlementStatus.Owned;
            }
            else
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][Level.OwnDLCLevelByLevelID] Invalid AdditionalContentModel");
#else
            if (!m_AdditionalContentModel)
                m_AdditionalContentModel = Resources.FindObjectsOfTypeAll<AdditionalContentModel>().FirstOrDefault();

            if (m_AdditionalContentModel)
            {
                m_GetLevelEntitlementStatusTokenSource?.Cancel();
                m_GetLevelEntitlementStatusTokenSource = new CancellationTokenSource();

                var l_Token = m_GetLevelEntitlementStatusTokenSource.Token;

                return await m_AdditionalContentModel.GetLevelEntitlementStatusAsync(p_LevelID, l_Token) == AdditionalContentModel.EntitlementStatus.Owned;
            }
            else
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][Level.OwnDLCLevelByLevelID] Invalid AdditionalContentModel");
#endif

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#if BEATSABER_1_35_0_OR_NEWER
        /// <summary>
        /// Try to get BeatmapLevel by level ID
        /// </summary>
        /// <param name="p_LevelID">ID of the level</param>
        /// <param name="p_BeatmapLevel">OUT Found BeatmapLevel or null</param>
        /// <returns>true or false </returns>
        public static bool TryGetBeatmapLevelForLevelID(string p_LevelID, out BeatmapLevel p_BeatmapLevel)
        {
            p_BeatmapLevel = null;

            var l_LevelID = SanitizeLevelID(p_LevelID);
            if (LevelID_IsCustom(l_LevelID))
            {
                if (SongCore.Loader.CustomLevels != null)
                {
                    var l_Custom = SongCore.Loader.CustomLevels.Select(x => x.Value).Where(x => x.levelID.Equals(l_LevelID, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (l_Custom != null)
                    {
                        p_BeatmapLevel = l_Custom;
                        return true;
                    }
                }
            }

            if (m_BeatmapLevelsModel == null)
                m_BeatmapLevelsModel = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().FirstOrDefault(x => x._beatmapLevelsModel != null)?._beatmapLevelsModel;

            if (m_BeatmapLevelsModel != null)
            {
                var l_Result = m_BeatmapLevelsModel.GetBeatmapLevel(l_LevelID);
                if (l_Result != null)
                {
                    p_BeatmapLevel = l_Result;
                    return true;
                }
            }
            else
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][Level.TryGetBeatmapLevelForLevelID] Invalid BeatmapLevelsModel");

            return false;
        }
        /// <summary>
        /// Try to get BeatmapLevel by Hash
        /// </summary>
        /// <param name="p_Hash">Hash of the level</param>
        /// <param name="p_BeatmapLevel">OUT Found BeatmapLevel or null</param>
        /// <returns>true or false </returns>
        public static bool TryGetBeatmapLevelForHash(string p_Hash, out BeatmapLevel p_BeatmapLevel)
        {
            p_BeatmapLevel = null;
            if (!TryGetLevelIDFromHash(p_Hash, out var l_LevelID))
                return false;

            return TryGetBeatmapLevelForLevelID(l_LevelID, out p_BeatmapLevel);
        }
#else
        /// <summary>
        /// Try to get PreviewBeatmapLevel by level ID
        /// </summary>
        /// <param name="p_LevelID">ID of the level</param>
        /// <param name="p_PreviewBeatmapLevel">OUT Found PreviewBeatmapLevel or null</param>
        /// <returns>true or false </returns>
        public static bool TryGetPreviewBeatmapLevelForLevelID(string p_LevelID, out IPreviewBeatmapLevel p_PreviewBeatmapLevel)
        {
            p_PreviewBeatmapLevel = null;

            var l_LevelID = SanitizeLevelID(p_LevelID);
            if (LevelID_IsCustom(l_LevelID) && SongCore.Loader.CustomLevelsCollection != null && SongCore.Loader.CustomLevelsCollection.beatmapLevels != null)
            {
                var l_Custom = SongCore.Loader.CustomLevelsCollection.beatmapLevels.Where(x => x.levelID == l_LevelID).FirstOrDefault();
                if (l_Custom != null)
                {
                    p_PreviewBeatmapLevel = l_Custom;
                    return true;
                }
            }

            if (!m_BeatmapLevelsModel)
                m_BeatmapLevelsModel = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().FirstOrDefault();

            if (m_BeatmapLevelsModel)
            {
                var l_Result = m_BeatmapLevelsModel.GetLevelPreviewForLevelId(l_LevelID);
                if (l_Result != null)
                {
                    p_PreviewBeatmapLevel = l_Result;
                    return true;
                }
            }
            else
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][Level.TryGetPreviewBeatmapLevelForLevelID] Invalid BeatmapLevelsModel");

            return false;
        }
        /// <summary>
        /// Try to get IPreviewBeatmapLevel by Hash
        /// </summary>
        /// <param name="p_Hash">Hash of the level</param>
        /// <param name="p_PreviewBeatmapLevel">OUT Found PreviewBeatmapLevel or null</param>
        /// <returns>true or false </returns>
        public static bool TryGetPreviewBeatmapLevelForHash(string p_Hash, out IPreviewBeatmapLevel p_PreviewBeatmapLevel)
        {
            p_PreviewBeatmapLevel = null;
            if (!TryGetLevelIDFromHash(p_Hash, out var l_LevelID))
                return false;

            return TryGetPreviewBeatmapLevelForLevelID(l_LevelID, out p_PreviewBeatmapLevel);
        }
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#if BEATSABER_1_35_0_OR_NEWER
        /// <summary>
        /// Check if a difficulty is present in a BeatmapLevel
        /// </summary>
        /// <param name="p_BeatmapLevel">Input beatmap level</param>
        /// <param name="p_BeatmapCharacteristicSO">Desired BeatmapCharacteristicSO</param>
        /// <param name="p_BeatmapDifficulty">Desired BeatmapDifficulty</param>
        /// <returns>True or false</returns>
        public static bool BeatmapLevel_HasDifficulty(BeatmapLevel p_BeatmapLevel, BeatmapCharacteristicSO p_BeatmapCharacteristicSO, BeatmapDifficulty p_BeatmapDifficulty)
        {
            if (p_BeatmapLevel == null || p_BeatmapCharacteristicSO == null)
                return false;

            return p_BeatmapLevel.GetDifficultyBeatmapData(p_BeatmapCharacteristicSO, p_BeatmapDifficulty) != null;
        }
        /// <summary>
        /// Try get a beatmap key from a BeatmapLevel
        /// </summary>
        /// <param name="p_BeatmapLevel">Input beatmap level</param>
        /// <param name="p_BeatmapCharacteristicSO">Desired BeatmapCharacteristicSO</param>
        /// <param name="p_BeatmapDifficulty">Desired BeatmapDifficulty</param>
        /// <param name="p_BeatmapKey">Out beatmap key</param>
        /// <returns>True or false</returns>
        public static bool BeatmapLevel_TryGetBeatmapKey(BeatmapLevel p_BeatmapLevel, BeatmapCharacteristicSO p_BeatmapCharacteristicSO, BeatmapDifficulty p_BeatmapDifficulty, out BeatmapKey p_BeatmapKey)
        {
            p_BeatmapKey = default;
            if (p_BeatmapLevel == null || p_BeatmapCharacteristicSO == null)
                return false;

            foreach (var l_BeatmapKey in p_BeatmapLevel.GetBeatmapKeys())
            {
                if (l_BeatmapKey.beatmapCharacteristic.serializedName != p_BeatmapCharacteristicSO.serializedName)
                    continue;
                if (l_BeatmapKey.difficulty != p_BeatmapDifficulty)
                    continue;

                p_BeatmapKey = l_BeatmapKey;
                return true;
            }

            return false;
        }
        /// <summary>
        /// Try get custom requirements for a BeatmapLevel->BeatmapCharacteristicSO->BeatmapDifficulty
        /// </summary>
        /// <param name="p_BeatmapLevel">Input beatmap level</param>
        /// <param name="p_BeatmapCharacteristicSO">Desired BeatmapCharacteristicSO</param>
        /// <param name="p_BeatmapDifficulty">Desired BeatmapDifficulty</param>
        /// <param name="p_CustomRequirements">OUT custom requirements</param>
        /// <returns>true or false</returns>
        public static bool TryGetCustomRequirementsFor( BeatmapLevel            p_BeatmapLevel,
                                                        BeatmapCharacteristicSO p_BeatmapCharacteristicSO,
                                                        BeatmapDifficulty       p_BeatmapDifficulty,
                                                        out List<string>        p_CustomRequirements)
        {
            p_CustomRequirements = null;
            if (p_BeatmapLevel == null || p_BeatmapCharacteristicSO == null)
                return false;

            if (!LevelID_IsCustom(p_BeatmapLevel.levelID)
                || !TryGetHashFromLevelID(p_BeatmapLevel.levelID, out var l_LevelHash))
                return false;

            var l_ExtraData = SongCore.Collections.RetrieveExtraSongData(l_LevelHash);
            if (l_ExtraData == null)
                return false;

            var l_CustomData = l_ExtraData._difficulties.FirstOrDefault((x) =>
            {
                return x._difficulty == p_BeatmapDifficulty
                        && (
                                x._beatmapCharacteristicName == p_BeatmapCharacteristicSO.characteristicNameLocalizationKey
                            ||
                                x._beatmapCharacteristicName == p_BeatmapCharacteristicSO.serializedName
                            );
            });

            if (l_CustomData == null)
                return false;

            p_CustomRequirements = new List<string>(l_CustomData.additionalDifficultyData._requirements);

            return true;
        }
#else
        /// <summary>
        /// Try get preview difficulty beatmap set by CharacteristicSO
        /// </summary>
        /// <param name="p_PreviewBeatmapLevel">Input preview beatmap level</param>
        /// <param name="p_BeatmapCharacteristicSO">Input characteristic SO</param>
        /// <param name="p_PreviewDifficultyBeatmapSet">OUT result preview beatmap set</param>
        /// <returns>True or false <summary></returns>
        public static bool TryGetPreviewDifficultyBeatmapSet(IPreviewBeatmapLevel p_PreviewBeatmapLevel, BeatmapCharacteristicSO p_BeatmapCharacteristicSO, out PreviewDifficultyBeatmapSet p_PreviewDifficultyBeatmapSet)
        {
            p_PreviewDifficultyBeatmapSet = null;

            var l_List  = p_PreviewBeatmapLevel.previewDifficultyBeatmapSets;
            var l_Count = l_List.Count;

            for (var l_I = 0; l_I < l_Count; ++l_I)
            {
                if (l_List[l_I].beatmapCharacteristic.serializedName != p_BeatmapCharacteristicSO.serializedName)
                    continue;

                p_PreviewDifficultyBeatmapSet = l_List[l_I];
                return true;
            }

            return false;
        }
        /// <summary>
        /// Check if a difficulty is present in a PreviewDifficultyBeatmapSet
        /// </summary>
        /// <param name="p_PreviewDifficultyBeatmapSet">Input PreviewDifficultyBeatmapSet</param>
        /// <param name="p_Difficulty">Requested difficulty</param>
        /// <returns>True or false</returns>
        public static bool PreviewDifficultyBeatmapSet_HasDifficulty(PreviewDifficultyBeatmapSet p_PreviewDifficultyBeatmapSet, BeatmapDifficulty p_Difficulty)
        {
            if (p_PreviewDifficultyBeatmapSet == null)
                return false;

            var l_Array = p_PreviewDifficultyBeatmapSet.beatmapDifficulties;
            var l_Count = l_Array.Length;

            for (var l_I = 0; l_I < l_Count; ++l_I)
            {
                if (l_Array[l_I] != p_Difficulty)
                    continue;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Try get custom requirements for a IPreviewBeatmapLevel->BeatmapCharacteristicSO->BeatmapDifficulty
        /// </summary>
        /// <param name="p_PreviewBeatmapLevel">Input preview beatmap level</param>
        /// <param name="p_BeatmapCharacteristicSO">Desired BeatmapCharacteristicSO</param>
        /// <param name="p_BeatmapDifficulty">Desired BeatmapDifficulty</param>
        /// <param name="p_CustomRequirements">OUT custom requirements</param>
        /// <returns>true or false</returns>
        public static bool TryGetCustomRequirementsFor(IPreviewBeatmapLevel     p_PreviewBeatmapLevel,
                                                        BeatmapCharacteristicSO p_BeatmapCharacteristicSO,
                                                        BeatmapDifficulty       p_BeatmapDifficulty,
                                                        out List<string>        p_CustomRequirements)
        {
            p_CustomRequirements = null;
            if (p_PreviewBeatmapLevel == null || p_BeatmapCharacteristicSO == null)
                return false;

            if (!LevelID_IsCustom(p_PreviewBeatmapLevel.levelID)
                || !TryGetHashFromLevelID(p_PreviewBeatmapLevel.levelID, out var l_LevelHash))
                return false;

            var l_ExtraData = SongCore.Collections.RetrieveExtraSongData(l_LevelHash);
            if (l_ExtraData == null)
                return false;

            var l_CustomData = l_ExtraData._difficulties.FirstOrDefault((x) =>
            {
                return x._difficulty == p_BeatmapDifficulty
                        && (
                                x._beatmapCharacteristicName == p_BeatmapCharacteristicSO.characteristicNameLocalizationKey
                            ||
                                x._beatmapCharacteristicName == p_BeatmapCharacteristicSO.serializedName
                            );
            });

            if (l_CustomData == null)
                return false;

            p_CustomRequirements = new List<string>(l_CustomData.additionalDifficultyData._requirements);

            return true;
        }
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#if BEATSABER_1_35_0_OR_NEWER
        /// <summary>
        /// Try to load BeatmapLevel cover image async
        /// </summary>
        /// <param name="p_BeatmapLevel">Input BeatmapLevel</param>
        /// <param name="p_Callback">Callback</param>
        public static void TryLoadBeatmapLevelCoverAsync(BeatmapLevel p_BeatmapLevel, Action<bool, Sprite> p_Callback)
        {
            if (p_BeatmapLevel == null || p_BeatmapLevel.previewMediaData == null)
            {
                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(false, GetDefaultPackCover()));
                return;
            }

            var l_CoverTask = null as Task<Sprite>;
            try
            {

#if BEATSABER_1_38_0_OR_NEWER
                l_CoverTask = p_BeatmapLevel.previewMediaData.GetCoverSpriteAsync();
#else
                l_CoverTask = p_BeatmapLevel.previewMediaData.GetCoverSpriteAsync(CancellationToken.None);
#endif
                l_CoverTask.ContinueWith((x) =>
                {
                    if (x != null && x.IsCompleted && x.Result)
                        CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(x.Result, x.Result));
                    else
                        CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(false, GetDefaultPackCover()));
                });
            }
            catch (Exception l_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.Game][Level.TryLoadBeatmapLevelCoverAsync] Error:");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);

                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(false, GetDefaultPackCover()));
                return;
            }
        }
#else
        /// <summary>
        /// Try to load PreviewBeatmapLevel cover image async
        /// </summary>
        /// <param name="p_PreviewBeatmapLevel">Input PreviewBeatmapLevel</param>
        /// <param name="p_Callback">Callback</param>
        public static void TryLoadPreviewBeatmapLevelCoverAsync(IPreviewBeatmapLevel p_PreviewBeatmapLevel, Action<bool, Sprite> p_Callback)
        {
            if (p_PreviewBeatmapLevel is CustomPreviewBeatmapLevel l_CustomPreviewBeatmapLevel)
            {
                var l_Existing = l_CustomPreviewBeatmapLevel._coverImage;
                if (l_Existing == null)
                {
                    var l_CoverImageFilename = l_CustomPreviewBeatmapLevel.standardLevelInfoSaveData.coverImageFilename;
                    if (!string.IsNullOrEmpty(l_CoverImageFilename))
                    {
                        var l_Path = Path.Combine(l_CustomPreviewBeatmapLevel.customLevelPath, l_CoverImageFilename);

                        CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() =>
                        {
                            try
                            {
                                var l_Bytes = File.ReadAllBytes(l_Path);
                                CP_SDK.Unity.SpriteU.CreateFromRawThreaded(l_Bytes, (x) =>
                                {
                                    l_CustomPreviewBeatmapLevel._coverImage = x ?? GetDefaultPackCover();
                                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(x, x ?? GetDefaultPackCover()));
                                });
                            }
                            catch (Exception)
                            {
                                l_CustomPreviewBeatmapLevel._coverImage = GetDefaultPackCover();
                                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(false, GetDefaultPackCover()));
                            }
                        });
                    }
                    else
                    {
                        l_CustomPreviewBeatmapLevel._coverImage = GetDefaultPackCover();
                        CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(false, GetDefaultPackCover()));
                    }
                }
                else
                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(l_CustomPreviewBeatmapLevel._coverImage, l_CustomPreviewBeatmapLevel._coverImage));
            }
            else
            {
                var l_CoverTask = null as Task<Sprite>;
                try
                {
                    l_CoverTask = p_PreviewBeatmapLevel.GetCoverImageAsync(CancellationToken.None);
                    l_CoverTask.ContinueWith((x) =>
                    {
                        if (x != null && x.IsCompleted && x.Result)
                            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(x.Result, x.Result));
                        else
                            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(false, GetDefaultPackCover()));
                    });
                }
                catch (Exception)
                {
                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(false, GetDefaultPackCover()));
                    return;
                }
            }
        }
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#if BEATSABER_1_35_0_OR_NEWER
        /// <summary>
        /// Load a BeatmapLevelData by level ID
        /// </summary>
        /// <param name="p_LevelID">ID of the level</param>
        /// <param name="p_LoadCallback">Load callback</param>
        public static async Task LoadBeatmapLevelDataByLevelID(string p_LevelID, Action<BeatmapLevel, IBeatmapLevelData> p_LoadCallback)
        {
            await Task.Yield();

            var l_LevelID = SanitizeLevelID(p_LevelID);
            if (!TryGetBeatmapLevelForLevelID(l_LevelID, out var l_BeatmapLevel))
            {
                p_LoadCallback(null, null);
                return;
            }

            if (!LevelID_IsCustom(p_LevelID))
            {
                if (!await OwnDLCLevelByLevelID(p_LevelID).ConfigureAwait(false))
                {
                    p_LoadCallback(null, null);
                    return; /// In the case of unowned DLC, just bail out and do nothing
                }
            }

            var l_Result = await LoadIBeatmapLevelDataAsync(p_LevelID).ConfigureAwait(false);
            if (l_Result != null && !(l_Result?.isError == true))
                p_LoadCallback(l_BeatmapLevel, l_Result.Value.beatmapLevelData);
            else
                p_LoadCallback(null, null);
        }
#else
        /// <summary>
        /// Load a BeatmapLevel by level ID
        /// </summary>
        /// <param name="p_LevelID">ID of the level</param>
        /// <param name="p_LoadCallback">Load callback</param>
        public static async Task LoadBeatmapLevelByLevelID(string p_LevelID, Action<IBeatmapLevel> p_LoadCallback)
        {
            await Task.Yield();

            var l_LevelID = SanitizeLevelID(p_LevelID);
            if (LevelID_IsCustom(l_LevelID))
            {
                var l_Level = SongCore.Loader.GetLevelById(l_LevelID);
                if (l_Level == null)
                {
                    p_LoadCallback(null);
                    return;
                }

                if (l_Level is CustomPreviewBeatmapLevel)
                {
                    var l_Result = await GetBeatmapLevelFromLevelID(l_Level.levelID).ConfigureAwait(false);
                    if (l_Result != null && !(l_Result?.isError == true))
                        p_LoadCallback(l_Result.Value.beatmapLevel);
                    else
                        p_LoadCallback(null);
                }
            }
            else
            {
                if (!await OwnDLCLevelByLevelID(l_LevelID).ConfigureAwait(false))
                {
                    p_LoadCallback(null);
                    return; /// In the case of unowned DLC, just bail out and do nothing
                }

                var l_Result = await GetBeatmapLevelFromLevelID(l_LevelID).ConfigureAwait(false);
                if (l_Result != null && !(l_Result?.isError == true))
                    p_LoadCallback(l_Result.Value.beatmapLevel);
                else
                    p_LoadCallback(null);
            }
        }
#endif
#if BEATSABER_1_35_0_OR_NEWER
        /// <summary>
        /// Start a BeatmapLevel
        /// </summary>
        /// <param name="p_Level">Loaded level</param>
        /// <param name="p_Characteristic">Beatmap game mode</param>
        /// <param name="p_Difficulty">Beatmap difficulty</param>
        /// <param name="p_BeatmapLevelData">Beatmap level data</param>
        /// <param name="p_OverrideEnvironmentSettings">Environment settings</param>
        /// <param name="p_ColorScheme">Color scheme</param>
        /// <param name="p_GameplayModifiers">Modifiers</param>
        /// <param name="p_PlayerSettings">Player settings</param>
        /// <param name="p_SongFinishedCallback">Callback when the song is finished</param>
        /// <param name="p_MenuButtonText">Menu button text</param>
        public static void StartBeatmapLevel(BeatmapLevel                    p_Level,
                                             BeatmapCharacteristicSO         p_Characteristic,
                                             BeatmapDifficulty               p_Difficulty,
                                             IBeatmapLevelData               p_BeatmapLevelData,
                                             OverrideEnvironmentSettings     p_OverrideEnvironmentSettings   = null,
                                             ColorScheme                     p_ColorScheme                   = null,
                                             GameplayModifiers               p_GameplayModifiers             = null,
                                             PlayerSpecificSettings          p_PlayerSettings                = null,
                                             Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> p_SongFinishedCallback = null,
                                             string                          p_MenuButtonText                = "Menu")
        {
            if (p_Level == null)
                return;

            if (!m_MenuTransitionsHelper)
                m_MenuTransitionsHelper = Resources.FindObjectsOfTypeAll<MenuTransitionsHelper>().First();

            if (!m_SimpleLevelStarter)
                m_SimpleLevelStarter = Resources.FindObjectsOfTypeAll<SimpleLevelStarter>().FirstOrDefault();

            if (m_MenuTransitionsHelper && m_SimpleLevelStarter)
            {
                try
                {
                    Scoring.BeatLeader_ManualWarmUpSubmission();

                    var l_BeatmapKey = p_Level.GetBeatmapKeys().FirstOrDefault(x => x.beatmapCharacteristic == p_Characteristic && x.difficulty == p_Difficulty);

#if !BEATSABER_1_38_0_OR_NEWER
                    PerformancePreset l_PerformancePreset = default(PerformancePreset);
                    if (!m_MenuTransitionsHelper._graphicSettingsHandler.TryGetCurrentPerformancePreset(out l_PerformancePreset))
                    {
                        Debug.LogError("Could not start level as the performance preset could not be decided.");
                        return;
                    }
#endif

                    /// Temp beatleader fix
                    m_MenuTransitionsHelper._standardLevelScenesTransitionSetupData.Init(
                        "Solo",
                        in l_BeatmapKey,
                        p_Level,
                        p_OverrideEnvironmentSettings,
                        p_ColorScheme,
                        null,
                        p_GameplayModifiers ?? new GameplayModifiers(),
                        p_PlayerSettings ?? new PlayerSpecificSettings(),
                        null,
                        m_SimpleLevelStarter._environmentsListModel,
                        m_MenuTransitionsHelper._audioClipAsyncLoader,
                        m_MenuTransitionsHelper._beatmapDataLoader,
#if BEATSABER_1_38_0_OR_NEWER
                        m_MenuTransitionsHelper._settingsManager,
#else
                        l_PerformancePreset,
#endif
                        p_MenuButtonText,
                        m_MenuTransitionsHelper._beatmapLevelsModel,
                        m_MenuTransitionsHelper._beatmapLevelsEntitlementModel,
                        false,
                        false,
                        null
                    );
                    m_MenuTransitionsHelper._standardLevelScenesTransitionSetupData.gameplayCoreSceneSetupData.beatmapLevelData = p_BeatmapLevelData;
                    m_MenuTransitionsHelper._standardLevelFinishedCallback = p_SongFinishedCallback;
                    m_MenuTransitionsHelper._standardLevelRestartedCallback = null;
                    m_MenuTransitionsHelper._standardLevelScenesTransitionSetupData.didFinishEvent -= m_MenuTransitionsHelper.HandleMainGameSceneDidFinish;
                    m_MenuTransitionsHelper._standardLevelScenesTransitionSetupData.didFinishEvent += m_MenuTransitionsHelper.HandleMainGameSceneDidFinish;
                    m_MenuTransitionsHelper._gameScenesManager.PushScenes(m_MenuTransitionsHelper._standardLevelScenesTransitionSetupData, 0.7f, null, null);

                    //m_MenuTransitionsHelper.StartStandardLevel(
                    //    /* string */                                                                    gameMode:                       "Solo",
                    //    /* in BeatmapKey */                                                             beatmapKey:                     l_BeatmapKey,
                    //    /* BeatmapLevel */                                                              beatmapLevel:                   p_Level,
                    //    /* IBeatmapLevelData */                                                         beatmapLevelData:               p_BeatmapLevelData,
                    //    /* OverrideEnvironmentSettings */                                               overrideEnvironmentSettings:    p_OverrideEnvironmentSettings,
                    //    /* ColorScheme */                                                               overrideColorScheme:            p_ColorScheme,
                    //    /* ColorScheme */                                                               beatmapOverrideColorScheme:     null,
                    //    /* GameplayModifiers */                                                         gameplayModifiers:              p_GameplayModifiers ?? new GameplayModifiers(),
                    //    /* PlayerSpecificSettings */                                                    playerSpecificSettings:         p_PlayerSettings    ?? new PlayerSpecificSettings(),
                    //    /* PracticeSettings */                                                          practiceSettings:               null,
                    //    /* EnvironmentsListModel */                                                     environmentsListModel:          m_SimpleLevelStarter._environmentsListModel,
                    //    /* string */                                                                    backButtonText:                 p_MenuButtonText,
                    //    /* bool */                                                                      useTestNoteCutSoundEffects:     false,
                    //    /* bool */                                                                      startPaused:                    false,
                    //    /* Action */                                                                    beforeSceneSwitchCallback:      null,
                    //    /* Action<DiContainer> */                                                       afterSceneSwitchCallback:       null,
                    //    /* Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>*/   levelFinishedCallback:          p_SongFinishedCallback,
                    //    /* Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>*/           levelRestartedCallback:         null,
                    //    /* RecordingToolManager.SetupData?*/                                            recordingToolData:              null
                    //);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.Game][Level.StartBeatmapLevel] Error:");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                }
            }
            else
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][Level.StartBeatmapLevel] Invalid MenuTransitionsHelper");
        }
#else
        /// <summary>
        /// Start a BeatmapLevel
        /// </summary>
        /// <param name="p_Level">Loaded level</param>
        /// <param name="p_Characteristic">Beatmap game mode</param>
        /// <param name="p_Difficulty">Beatmap difficulty</param>
        /// <param name="p_OverrideEnvironmentSettings">Environment settings</param>
        /// <param name="p_ColorScheme">Color scheme</param>
        /// <param name="p_GameplayModifiers">Modifiers</param>
        /// <param name="p_PlayerSettings">Player settings</param>
        /// <param name="p_SongFinishedCallback">Callback when the song is finished</param>
        /// <param name="p_MenuButtonText">Menu button text</param>
        public static void StartBeatmapLevel(IBeatmapLevel                   p_Level,
                                             BeatmapCharacteristicSO         p_Characteristic,
                                             BeatmapDifficulty               p_Difficulty,
                                             OverrideEnvironmentSettings     p_OverrideEnvironmentSettings   = null,
                                             ColorScheme                     p_ColorScheme                   = null,
                                             GameplayModifiers               p_GameplayModifiers             = null,
                                             PlayerSpecificSettings          p_PlayerSettings                = null,
                                             Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> p_SongFinishedCallback = null,
                                             string                          p_MenuButtonText                = "Menu")
        {
            if (p_Level == null || p_Level.beatmapLevelData == null)
                return;

            if (!m_MenuTransitionsHelper)
                m_MenuTransitionsHelper = Resources.FindObjectsOfTypeAll<MenuTransitionsHelper>().First();

            if (m_MenuTransitionsHelper)
            {
                try
                {
                    Scoring.BeatLeader_ManualWarmUpSubmission();

                    var l_DifficultyBeatmap = p_Level.beatmapLevelData.GetDifficultyBeatmap(p_Characteristic, p_Difficulty);

                    m_MenuTransitionsHelper.StartStandardLevel(
                        gameMode:                       "Solo",
                        difficultyBeatmap:              l_DifficultyBeatmap,
                        previewBeatmapLevel:            p_Level,
                        overrideEnvironmentSettings:    p_OverrideEnvironmentSettings,
                        overrideColorScheme:            p_ColorScheme,
                        gameplayModifiers:              p_GameplayModifiers ?? new GameplayModifiers(),
                        playerSpecificSettings:         p_PlayerSettings    ?? new PlayerSpecificSettings(),
                        practiceSettings:               null,
                        backButtonText:                 p_MenuButtonText,
                        useTestNoteCutSoundEffects:     false,
                        startPaused:                    false,
                        beforeSceneSwitchCallback:      null,
                        afterSceneSwitchCallback:       null,
                        levelFinishedCallback:          (p_StandardLevelScenesTransitionSetupData, p_Results) => p_SongFinishedCallback?.Invoke(p_StandardLevelScenesTransitionSetupData, p_Results),
                        levelRestartedCallback:         null
                    );
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.Game][Level.StartBeatmapLevel] Error:");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                }
            }
            else
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][Level.StartBeatmapLevel] Invalid MenuTransitionsHelper");
        }
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#if BEATSABER_1_35_0_OR_NEWER
        /// <summary>
        /// Load IBeatmapLevelData from a level ID
        /// </summary>
        /// <param name="p_LevelID">Level ID</param>
        /// <returns>LoadBeatmapLevelDataResult?</returns>
        private static async Task<LoadBeatmapLevelDataResult?> LoadIBeatmapLevelDataAsync(string p_LevelID)
        {
            if (m_BeatmapLevelsModel == null)
                m_BeatmapLevelsModel = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().FirstOrDefault(x => x._beatmapLevelsModel != null)?._beatmapLevelsModel;

            if (!m_MenuTransitionsHelper)
                m_MenuTransitionsHelper = Resources.FindObjectsOfTypeAll<MenuTransitionsHelper>().First();

            if (m_BeatmapLevelsModel != null)
            {
                m_GetLevelCancellationTokenSource?.Cancel();
                m_GetLevelCancellationTokenSource = new CancellationTokenSource();

                var l_Token = m_GetLevelCancellationTokenSource.Token;

                LoadBeatmapLevelDataResult? l_Result = null;
                try
                {
                    var l_BeatmapLevelDataVersion = await m_MenuTransitionsHelper._beatmapLevelsEntitlementModel.GetLevelDataVersionAsync(p_LevelID, l_Token);
                    l_Token.ThrowIfCancellationRequested();
                    l_Result = await m_BeatmapLevelsModel.LoadBeatmapLevelDataAsync(p_LevelID, l_BeatmapLevelDataVersion, l_Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.Game][Level.LoadIBeatmapLevelDataAsync] Error:");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                }

                if (l_Result?.isError == true || l_Result?.beatmapLevelData == null)
                    return null; /// Null out entirely in case of error

                return l_Result;
            }
            else
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][Level.LoadIBeatmapLevelDataAsync] Invalid BeatmapLevelsModel");

            return null;
        }
#else
        /// <summary>
        /// Get a BeatmapLevel from a level ID
        /// </summary>
        /// <param name="p_LevelID">Level ID</param>
        /// <returns>GetBeatmapLevelResult?</returns>
        private static async Task<BeatmapLevelsModel.GetBeatmapLevelResult?> GetBeatmapLevelFromLevelID(string p_LevelID)
        {
            if (!m_BeatmapLevelsModel)
                m_BeatmapLevelsModel = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().FirstOrDefault();

            if (m_BeatmapLevelsModel)
            {
                m_GetLevelCancellationTokenSource?.Cancel();
                m_GetLevelCancellationTokenSource = new CancellationTokenSource();

                var l_Token = m_GetLevelCancellationTokenSource.Token;

                BeatmapLevelsModel.GetBeatmapLevelResult? l_Result = null;
                try
                {
                    l_Result = await m_BeatmapLevelsModel.GetBeatmapLevelAsync(p_LevelID, l_Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {

                }

                if (l_Result?.isError == true || l_Result?.beatmapLevel == null)
                    return null; /// Null out entirely in case of error

                return l_Result;
            }
            else
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][Level.GetBeatmapLevelFromLevelID] Invalid BeatmapLevelsModel");

            return null;
        }
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get accuracy
        /// </summary>
        /// <param name="p_MaxScore">Max score</param>
        /// <param name="p_Score">Result score</param>
        /// <returns></returns>
        public static float GetAccuracy(int p_MaxScore, int p_Score)
        {
            return (float)Math.Round(1.0 / (double)p_MaxScore * (double)p_Score, 4);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#if BEATSABER_1_35_0_OR_NEWER
        /// <summary>
        /// Get scores from local cache for a level id
        /// </summary>
        /// <param name="p_LevelID">Level ID</param>
        /// <param name="p_HaveAnyScore">Have any score set</param>
        /// <param name="p_HaveAllScores">Have all scores set</param>
        /// <returns>Scores</returns>
        public static Dictionary<BeatmapCharacteristicSO, List<(BeatmapDifficulty, int)>> GetScoresByLevelID(string p_LevelID, out bool p_HaveAnyScore, out bool p_HaveAllScores)
        {
            var l_Results = new Dictionary<BeatmapCharacteristicSO, List<(BeatmapDifficulty, int)>>();

            p_HaveAnyScore  = false;
            p_HaveAllScores = true;

            var l_LevelID = SanitizeLevelID(p_LevelID);
            if (!TryGetBeatmapLevelForLevelID(l_LevelID, out var l_BeatmapLevel))
            {
                p_HaveAllScores = false;
                return l_Results;
            }

            var l_PlayerDataModel   = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault();
            var l_PlayerData        = l_PlayerDataModel?._playerData;
            var l_LevelStatsData    = l_PlayerData?.levelsStatsData;
            foreach (var l_BeatmapKey in l_BeatmapLevel.GetBeatmapKeys())
            {
                if (!l_Results.ContainsKey(l_BeatmapKey.beatmapCharacteristic))
                    l_Results.Add(l_BeatmapKey.beatmapCharacteristic, new List<(BeatmapDifficulty, int)>());

                if (l_LevelStatsData.TryGetValue(l_BeatmapKey, out var l_PlayerLevelStatsData) && l_PlayerLevelStatsData.playCount > 0)
                {
                    p_HaveAnyScore = true;
                    l_Results[l_BeatmapKey.beatmapCharacteristic].Add((l_BeatmapKey.difficulty, l_PlayerLevelStatsData.highScore));
                }
                else
                {
                    p_HaveAllScores = false;
                    l_Results[l_BeatmapKey.beatmapCharacteristic].Add((l_BeatmapKey.difficulty, -1));
                }
            }

            return l_Results;
        }
#else
        /// <summary>
        /// Get scores from local cache for a level id
        /// </summary>
        /// <param name="p_LevelID">Level ID</param>
        /// <param name="p_HaveAnyScore">Have any score set</param>
        /// <param name="p_HaveAllScores">Have all scores set</param>
        /// <returns>Scores</returns>
        public static Dictionary<BeatmapCharacteristicSO, List<(BeatmapDifficulty, int)>> GetScoresByLevelID(string p_LevelID, out bool p_HaveAnyScore, out bool p_HaveAllScores)
        {
            var l_Results = new Dictionary<BeatmapCharacteristicSO, List<(BeatmapDifficulty, int)>>();

            p_HaveAnyScore  = false;
            p_HaveAllScores = true;

            var l_LevelID = SanitizeLevelID(p_LevelID);
            if (!TryGetPreviewBeatmapLevelForLevelID(l_LevelID, out var l_PreviewBeatmapLevel))
            {
                p_HaveAllScores = false;
                return l_Results;
            }

            var l_PlayerDataModel = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault();
            foreach (var l_DifficultySet in l_PreviewBeatmapLevel.previewDifficultyBeatmapSets)
            {
                if (!l_Results.ContainsKey(l_DifficultySet.beatmapCharacteristic))
                    l_Results.Add(l_DifficultySet.beatmapCharacteristic, new List<(BeatmapDifficulty, int)>());

                foreach (var l_Difficulty in l_DifficultySet.beatmapDifficulties)
                {
                    var l_ScoreSO = l_PlayerDataModel.playerData.GetPlayerLevelStatsData(l_LevelID, l_Difficulty, l_DifficultySet.beatmapCharacteristic);

                    if (l_ScoreSO.validScore)
                    {
                        p_HaveAnyScore = true;
                        l_Results[l_DifficultySet.beatmapCharacteristic].Add((l_Difficulty, l_ScoreSO.highScore));
                    }
                    else
                    {
                        p_HaveAllScores = false;
                        l_Results[l_DifficultySet.beatmapCharacteristic].Add((l_Difficulty, -1));
                    }
                }
            }

            return l_Results;
        }
#endif
    }
}
