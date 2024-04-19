﻿using HarmonyLib;
using IPA.Utilities;

namespace CP_SDK_BS.Game.Patches
{
    /// <summary>
    /// Level data finder
    /// </summary>
    [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO))]
#if BEATSABER_1_35_0_OR_NEWER
    [HarmonyPatch(nameof(StandardLevelScenesTransitionSetupDataSO.InitAndSetupScenes))]
#else
    [HarmonyPatch(nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
#endif
    public class PStandardLevelScenesTransitionSetupDataSO : StandardLevelScenesTransitionSetupDataSO
    {
        /// <summary>
        /// Level data cache
        /// </summary>
        static private LevelData m_LevelData = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Postfix
        /// </summary>
        internal static void Postfix(ref StandardLevelScenesTransitionSetupDataSO __instance)
        {
            m_LevelData = new LevelData()
            {
                Type = LevelType.Solo,
                Data = __instance.gameplayCoreSceneSetupData
            };

            Logic.FireLevelStarted(m_LevelData);

            __instance.didFinishEvent -= OnDidFinishEvent;
            __instance.didFinishEvent += OnDidFinishEvent;
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
        private static void OnDidFinishEvent(StandardLevelScenesTransitionSetupDataSO p_Transition, LevelCompletionResults p_LevelCompletionResult)
        {
            if (m_LevelData == null)
                return;

            Logic.FireLevelEnded(new LevelCompletionData() { Type = LevelType.Solo, Data = m_LevelData.Data, Results = p_LevelCompletionResult });
            m_LevelData = null;
        }
    }
}
