﻿using IPA.Loader;
using System.Reflection;

namespace CP_SDK_BS.Game
{
    /// <summary>
    /// Scoring utils
    /// </summary>
    public static class Scoring
    {
        private static bool m_Init;

        private static bool         m_IsScoreSaberPresent;
        private static MethodBase   m_ScoreSaber_playbackEnabled;

        private static bool         m_IsBeatLeaderPresent;
        private static MethodBase   m_BeatLeader_RecorderUtils_OnActionButtonWasPressed;
        private static PropertyInfo m_BeatLeader_ReplayerMenuLauncher_IsStartedAsReplay;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static bool IsScoreSaberPresent  { get { Init(); return m_IsScoreSaberPresent; } }
        public static bool IsBeatLeaderPresent  { get { Init(); return m_IsBeatLeaderPresent; } }

        public static bool IsInReplay           { get { return ScoreSaber_IsInReplay() || BeatLeader_IsInReplay(); } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is in ScoreSaber replay
        /// </summary>
        /// <returns></returns>
        public static bool ScoreSaber_IsInReplay()
        {
            Init();

            if (!m_IsScoreSaberPresent || m_ScoreSaber_playbackEnabled == null)
                return false;

            return !(bool)m_ScoreSaber_playbackEnabled.Invoke(null, null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is in BeatLeader replay
        /// </summary>
        /// <returns></returns>
        public static bool BeatLeader_IsInReplay()
        {
            Init();

            if (!m_IsBeatLeaderPresent || m_BeatLeader_ReplayerMenuLauncher_IsStartedAsReplay == null)
                return false;

            return (bool)m_BeatLeader_ReplayerMenuLauncher_IsStartedAsReplay.GetValue(null);
        }
        /// <summary>
        /// Warmup BeatLeader score submission
        /// </summary>
        public static void BeatLeader_ManualWarmUpSubmission()
        {
            Init();

            if (!m_IsBeatLeaderPresent || m_BeatLeader_RecorderUtils_OnActionButtonWasPressed == null)
                return;

            m_BeatLeader_RecorderUtils_OnActionButtonWasPressed.Invoke(null, null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init scoring utils
        /// </summary>
        private static void Init()
        {
            if (m_Init)
                return;

            var l_ScoreSaberMetaData = PluginManager.GetPluginFromId("ScoreSaber");
            if (l_ScoreSaberMetaData != null)
            {
                m_ScoreSaber_playbackEnabled = l_ScoreSaberMetaData.Assembly.GetType("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted")?
                                                .GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic);

                m_IsScoreSaberPresent = true;
            }
            else
                m_IsScoreSaberPresent = false;

            var l_BeatLeaderMetaData = PluginManager.GetPluginFromId("BeatLeader");
            if (l_BeatLeaderMetaData != null)
            {
                m_BeatLeader_RecorderUtils_OnActionButtonWasPressed = l_BeatLeaderMetaData.Assembly.GetType("BeatLeader.Utils.RecorderUtils")?
                                                                      .GetMethod("OnActionButtonWasPressed", BindingFlags.Static | BindingFlags.NonPublic);

                m_BeatLeader_ReplayerMenuLauncher_IsStartedAsReplay = l_BeatLeaderMetaData.Assembly.GetType("BeatLeader.Replayer.ReplayerLauncher")?
                                                                      .GetProperty("IsStartedAsReplay", BindingFlags.Static | BindingFlags.Public);

                m_IsBeatLeaderPresent = true;
            }
            else
                m_IsBeatLeaderPresent = false;

            m_Init = true;
        }
    }
}
