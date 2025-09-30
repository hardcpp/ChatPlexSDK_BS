using IPA.Loader;
using System;
using System.Reflection;

namespace CP_SDK_BS.Game
{
    /// <summary>
    /// BetterSongSearch utils
    /// </summary>
    public static class BetterSongSearch
    {
        private static bool m_Init;

        private static bool m_IsPresent;
        private static MethodBase m_Manager_ShowFlow;
        private static MethodBase m_BSSFlowCoordinator_Close;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is mod installed
        /// </summary>
        public static bool IsPresent { get { Init(); return m_IsPresent; } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show BetterSongSearch flow
        /// </summary>
        public static void Manager_ShowFlow()
        {
            Init();

            if (!m_IsPresent || m_Manager_ShowFlow == null)
                return;

            try { m_Manager_ShowFlow.Invoke(null, new object[] {}); } catch { }
        }
        /// <summary>
        /// Show BetterSongSearch flow
        /// </summary>
        public static void BSSFlowCoordinator_Close()
        {
            Init();

            if (!m_IsPresent || m_BSSFlowCoordinator_Close == null)
                return;

            try { m_BSSFlowCoordinator_Close.Invoke(null, new object[] { true, false }); } catch { }
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

            var l_MetaData = PluginManager.GetPluginFromId("BetterSongSearch");
            if (l_MetaData != null)
            {
                try
                {
                    m_Manager_ShowFlow = l_MetaData.Assembly.GetType("BetterSongSearch.UI.Manager")?
                                                .GetMethod("ShowFlow", BindingFlags.Static | BindingFlags.Public, null, new Type[] { }, null);

                    m_BSSFlowCoordinator_Close = l_MetaData.Assembly.GetType("BetterSongSearch.UI.BSSFlowCoordinator")?
                                                .GetMethod("Close", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(bool), typeof(bool) }, null);
                }
                catch (Exception) { }

                m_IsPresent = m_Manager_ShowFlow != null && m_BSSFlowCoordinator_Close != null;
            }
            else
                m_IsPresent = false;

            m_Init = true;
        }
    }
}
