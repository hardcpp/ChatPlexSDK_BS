using CP_SDK;
using CP_SDK.Unity;
using CP_SDK_WebSocketSharp;
using System;
using System.Threading;
using UnityEngine;

namespace CP_SDK_BS.Game
{
    /// <summary>
    /// UserPlatform helper
    /// </summary>
    public static class UserPlatform
    {
        /// <summary>
        /// User ID cache
        /// </summary>
        private static string m_UserID = null;
        /// <summary>
        /// User name cache
        /// </summary>
        private static string m_UserName = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get User ID
        /// </summary>
        /// <returns></returns>
        public static string GetUserID()
        {
            if (m_UserID != null)
                return m_UserID;

            if (MTMainThreadInvoker.IsMainThread())
                FetchPlatformInfos();
            else
            {
                var isReady = false;
                MTMainThreadInvoker.Enqueue(() =>
                {
                    FetchPlatformInfos();
                    isReady = true;
                });

                while (!isReady)
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
            }

            return m_UserID;
        }
        /// <summary>
        /// Get User Name
        /// </summary>
        /// <returns></returns>
        public static string GetUserName()
        {
            if (m_UserName != null)
                return m_UserName;

            if (MTMainThreadInvoker.IsMainThread())
                FetchPlatformInfos();
            else
            {
                var isReady = false;
                MTMainThreadInvoker.Enqueue(() =>
                {
                    FetchPlatformInfos();
                    isReady = true;
                });

                while (!isReady)
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
            }

            return m_UserName;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Find platform informations
        /// </summary>
        private static void FetchPlatformInfos()
        {
            try
            {
                var l_PlatformLeaderboardsModels = Resources.FindObjectsOfTypeAll<PlatformLeaderboardsModel>();

                foreach (var l_Current in l_PlatformLeaderboardsModels)
                {
                    var l_PlatformUserModel = l_Current._platform;
                    if (l_PlatformUserModel == null)
                        continue;

                    l_Current.Initialize();

                    if (l_Current.playerId != 0)
                    {
                        m_UserID = l_Current.playerId.ToString();
                        m_UserName = l_Current._platform.user.displayName;
                        return;
                    }
                }
            }
            catch (System.Exception l_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][UserPlatform] Unable to find user platform informations");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
