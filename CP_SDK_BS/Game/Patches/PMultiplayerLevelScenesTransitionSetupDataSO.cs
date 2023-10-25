﻿using HarmonyLib;
using IPA.Utilities;

namespace CP_SDK_BS.Game.Patches
{
    /// <summary>
    /// Level data finder
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO))]
    [HarmonyPatch(nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
    public class PMultiplayerLevelScenesTransitionSetupDataSO : MultiplayerLevelScenesTransitionSetupDataSO
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
        internal static void Postfix(ref MultiplayerLevelScenesTransitionSetupDataSO __instance)
        {
            var l_LevelData = new LevelData()
            {
                Type = LevelType.Multiplayer,
                Data = __instance.gameplayCoreSceneSetupData
            };

            Logic.FireLevelStarted(l_LevelData);

            m_LevelData = l_LevelData;

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
        /// <param name="p_OtherPlayersLevelCompletionResults">Other player results</param>
        private static void OnDidFinishEvent(MultiplayerLevelScenesTransitionSetupDataSO p_Transition, MultiplayerResultsData p_MultiplayerResultsData)
        {
            if (m_LevelData == null)
                return;

            Logic.FireLevelEnded(new LevelCompletionData() { Type = LevelType.Multiplayer, Data = m_LevelData.Data, Results = p_MultiplayerResultsData?.localPlayerResultData?.multiplayerLevelCompletionResults?.levelCompletionResults ?? null });
            m_LevelData = null;
        }
    }
}
