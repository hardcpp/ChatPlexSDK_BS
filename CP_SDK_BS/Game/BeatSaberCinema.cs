using IPA.Loader;
using System;
using System.Reflection;

namespace CP_SDK_BS.Game
{
    /// <summary>
    /// BeatSaberCinema utils
    /// </summary>
    public static class BeatSaberCinema
    {
        /// <summary>
        /// Is initialized
        /// </summary>
        private static bool m_Init;

        /// <summary>
        /// Is present
        /// </summary>
        private static bool m_IsPresent;
        /// <summary>
        /// SetSelectedLevel
        /// </summary>
        private static MethodBase m_Events_SetSelectedLevel;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is mod installed
        /// </summary>
        public static bool IsPresent { get { Init(); return m_IsPresent; } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set selected level for BeatSaberCinema mod
        /// </summary>
#if BEATSABER_1_35_0_OR_NEWER
        public static void SetSelectedLevel(BeatmapLevel p_PreviewBeatMapLevel)
#else
        public static void SetSelectedLevel(IPreviewBeatmapLevel p_PreviewBeatMapLevel)
#endif
        {
            Init();

            if (!m_IsPresent || m_Events_SetSelectedLevel == null)
                return;

            try { m_Events_SetSelectedLevel.Invoke(null, new object[] { p_PreviewBeatMapLevel }); } catch { }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        private static void Init()
        {
            if (m_Init)
                return;

            var l_MetaData = PluginManager.GetPluginFromId("BeatSaberCinema");
            if (l_MetaData != null)
            {
                try
                {
#if BEATSABER_1_35_0_OR_NEWER
                    m_Events_SetSelectedLevel = l_MetaData.Assembly.GetType("BeatSaberCinema.Events")?
                                                .GetMethod("SetSelectedLevel", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(BeatmapLevel) }, null);
#else
                    m_Events_SetSelectedLevel = l_MetaData.Assembly.GetType("BeatSaberCinema.Events")?
                                                .GetMethod("SetSelectedLevel", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(IPreviewBeatmapLevel) }, null);
#endif
                }
                catch (Exception) { }

                m_IsPresent = m_Events_SetSelectedLevel != null;
            }
            else
                m_IsPresent = false;

            m_Init = true;
        }
    }
}
