﻿#if BEATSABER_1_35_0_OR_NEWER
using HarmonyLib;

namespace CP_SDK_BS.Game.Patches
{
    /// <summary>
    /// Level data finder
    /// </summary>
    [HarmonyPatch(typeof(MenuTransitionsHelper))]
    [HarmonyPatch(nameof(MenuTransitionsHelper.StartMissionLevel))]
    public class PMenuTransitionsHelper__StartMissionLevel
    {
        /// <summary>
        /// Level data cache
        /// </summary>
        static private LevelData m_LevelData = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        internal static void Postfix(ref MenuTransitionsHelper __instance)
        {
            var l_LevelData = new LevelData()
            {
                Type = LevelType.Solo,
                Data = __instance._missionLevelScenesTransitionSetupData.gameplayCoreSceneSetupData
            };

            Logic.FireLevelStarted(l_LevelData);

            m_LevelData = l_LevelData;

            __instance._missionLevelScenesTransitionSetupData.didFinishEvent -= OnDidFinishEvent;
            __instance._missionLevelScenesTransitionSetupData.didFinishEvent += OnDidFinishEvent;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Restore the level data (Fix for the new restart mechanic)
        /// </summary>
        /// <param name="p_LevelData">Level data to restore</param>
        internal static void RestoreLevelData(LevelData p_LevelData)
            => m_LevelData = p_LevelData;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On level finish
        /// </summary>
        /// <param name="p_Transition">Transition data</param>
        /// <param name="p_LevelCompletionResult">Completion result</param>
        private static void OnDidFinishEvent(MissionLevelScenesTransitionSetupDataSO p_Transition, MissionCompletionResults p_LevelCompletionResult)
        {
            if (m_LevelData == null)
                return;

            Logic.FireLevelEnded(new LevelCompletionData() { Type = LevelType.Solo, Data = m_LevelData.Data, Results = p_LevelCompletionResult.levelCompletionResults });
            m_LevelData = null;
        }
    }
}
#endif