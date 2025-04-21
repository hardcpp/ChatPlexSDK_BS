using BSP_ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CP_SDK_BS.Game
{
    /// <summary>
    /// BeatMaps client
    /// </summary>
    public class BeatMapsClient
    {
        private static string                           m_CacheFolder;
        private static bool                             m_CacheEnabled = true;
        private static CP_SDK.Network.WebClientUnity    m_WebClient;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BeatMaps client
        /// </summary>
        public static CP_SDK.Network.WebClientUnity WebClient => m_WebClient;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init the BeatMaps client
        /// </summary>
        internal static void Init()
        {
            m_CacheFolder               = $"UserData/{CP_SDK.ChatPlexSDK.ProductName}Plus/Cache/BeatMaps/";
            m_WebClient                 = new CP_SDK.Network.WebClientUnity("", TimeSpan.FromSeconds(10));
            m_WebClient.DownloadTimeout = 2 * 60;

            try
            {
                if (!Directory.Exists(m_CacheFolder))
                    Directory.CreateDirectory(m_CacheFolder);

                m_CacheEnabled = true;
            }
            catch (Exception l_Exception)
            {
                m_CacheEnabled = false;

                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK.Chat][ImageProvider.Init] Error creating cache folder, disabling caching:");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get online by key
        /// </summary>
        /// <param name="p_Key">Key</param>
        /// <param name="p_Callback">Callback (p_Success, p_BeatMap)</param>
        public static void GetOnlineByKey(string p_Key, Action<bool, BeatMaps.MapDetail> p_Callback)
        {
            m_WebClient.GetAsync("https://api.beatsaver.com/maps/id/" + p_Key, CancellationToken.None, (p_Result) =>
            {
                try
                {
                    if (!p_Result.IsSuccessStatusCode
                        || !GetObjectFromJsonString<BeatMaps.MapDetail>(p_Result.BodyString, out var l_BeatMap))
                    {
                        p_Callback?.Invoke(p_Result.StatusCode == System.Net.HttpStatusCode.NotFound, null);
                        return;
                    }

                    l_BeatMap.Partial = false;
                    p_Callback?.Invoke(true, l_BeatMap);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient.GetOnlineByKey] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_Callback?.Invoke(false, null);
                }
            });
        }
        /// <summary>
        /// Get online by hash
        /// </summary>
        /// <param name="p_Hash">Hash</param>
        /// <param name="p_Callback">Callback (p_Success, p_BeatMap)</param>
        public static void GetOnlineByHash(string p_Hash, Action<bool, BeatMaps.MapDetail> p_Callback)
        {
            m_WebClient.GetAsync("https://api.beatsaver.com/maps/hash/" + p_Hash, CancellationToken.None, (p_Result) =>
            {
                try
                {
                    if (!p_Result.IsSuccessStatusCode
                        || !GetObjectFromJsonString<BeatMaps.MapDetail>(p_Result.BodyString, out var l_BeatMap))
                    {
                        p_Callback?.Invoke(p_Result.StatusCode == System.Net.HttpStatusCode.NotFound, null);
                        return;
                    }

                    l_BeatMap.Partial = false;
                    p_Callback?.Invoke(true, l_BeatMap);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient.GetOnlineByHash] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_Callback?.Invoke(false, null);
                }
            });
        }
        /// <summary>
        /// Get online by search
        /// </summary>
        /// <param name="p_Query">Search query</param>
        /// <param name="p_Callback">Callback (p_BeatMaps)</param>
        public static void GetOnlineBySearch(string p_Query, Action<bool, BeatMaps.MapDetail[]> p_Callback)
        {
            m_WebClient.GetAsync("https://api.beatsaver.com/search/text/0?sortOrder=Relevance&q=" + CP_SDK_WebSocketSharp.Net.HttpUtility.UrlEncode(p_Query), CancellationToken.None, (p_Result) =>
            {
                try
                {
                    if (!p_Result.IsSuccessStatusCode
                        || !GetObjectFromJsonString<BeatMaps.SearchResponse>(p_Result.BodyString, out var l_SearchResult))
                    {
                        p_Callback?.Invoke(false, null);
                        return;
                    }

                    for (int l_I = 0; l_I < l_SearchResult.docs.Length; ++l_I)
                        l_SearchResult.docs[l_I].Partial = false;

                    p_Callback?.Invoke(true, l_SearchResult.docs);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient.GetOnlineBySearch] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_Callback?.Invoke(false, null);
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Populate partial BeatMap by key
        /// </summary>
        /// <param name="p_BeatMap">BeatMap to populate</param>
        /// <param name="p_Callback">Callback (p_Success)</param>
        public static void PopulateOnlineByKey(BeatMaps.MapDetail p_BeatMap, Action<bool> p_Callback)
        {
            m_WebClient.GetAsync("https://api.beatsaver.com/maps/id/" + p_BeatMap.id, CancellationToken.None, (p_Result) =>
            {
                try
                {
                    if (!p_Result.IsSuccessStatusCode)
                    {
                        p_Callback?.Invoke(false);
                        return;
                    }

                    JsonConvert.PopulateObject(p_Result.BodyString, p_BeatMap);
                    p_BeatMap.Partial = false;
                    p_Callback?.Invoke(true);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient.PopulateOnlineByKey] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_Callback?.Invoke(false);
                }
            });
        }
        /// <summary>
        /// Populate partial BeatMap by hash
        /// </summary>
        /// <param name="p_BeatMap">BeatMap to populate</param>
        /// <param name="p_Callback">Callback (p_Success)</param>
        public static void PopulateOnlineByHash(BeatMaps.MapDetail p_BeatMap, Action<bool> p_Callback)
        {
            m_WebClient.GetAsync("https://api.beatsaver.com/maps/hash/" + p_BeatMap.PartialHash, CancellationToken.None, (p_Result) =>
            {
                try
                {
                    if (!p_Result.IsSuccessStatusCode)
                    {
                        p_Callback?.Invoke(false);
                        return;
                    }

                    JsonConvert.PopulateObject(p_Result.BodyString, p_BeatMap);
                    p_BeatMap.Partial = false;
                    p_Callback?.Invoke(true);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient.PopulateOnlineByHash] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_Callback?.Invoke(false);
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get from cache
        /// </summary>
        /// <param name="p_Key">Key</param>
        /// <returns></returns>
        public static BeatMaps.MapDetail GetFromCacheByKey(string p_Key)
        {
            try
            {
                var l_Path = m_CacheFolder + p_Key + ".json";

                if (!File.Exists(l_Path))
                    return null;

                var l_Content = File.ReadAllText(l_Path, Encoding.UTF8);
                var l_Result = JsonConvert.DeserializeObject<BeatMaps.MapDetail>(l_Content);
                l_Result.Partial = false;

                return l_Result;
            }
            catch (Exception l_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient.GetFromCacheByKey] Error :");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
            }

            return null;
        }
        /// <summary>
        /// Get cover image from cache
        /// </summary>
        /// <param name="p_Key">Key</param>
        /// <returns></returns>
        public static byte[] GetCoverImageFromCacheByKey(string p_Key)
        {
            try
            {
                var l_Path = m_CacheFolder + p_Key + ".jpg";

                if (!File.Exists(l_Path))
                    return null;

                var l_Result = File.ReadAllBytes(l_Path);

                return l_Result;
            }
            catch (Exception l_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient.GetCoverImageFromCacheByKey] Error :");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cache instance
        /// </summary>
        /// <param name="p_MapDetails">Instance to cache</param>
        public static void Cache(BeatMaps.MapDetail p_MapDetails)
        {
            if (p_MapDetails == null || !p_MapDetails.IsValid())
                return;

            WriteCacheTextFile(p_MapDetails.id + ".json", p_MapDetails);
        }
        /// <summary>
        /// Cache instance cover image
        /// </summary>
        /// <param name="p_MapDetails">Instance to cache</param>
        /// <param name="p_Cover">Cover bytes</param>
        public static void CacheCoverImage(BeatMaps.MapDetail p_MapDetails, byte[] p_Cover)
        {
            if (p_MapDetails == null || !p_MapDetails.IsValid() || p_Cover.Length == 0)
                return;

            WriteCacheFile(p_MapDetails.id + ".jpg", p_Cover);
        }
        /// <summary>
        /// Clear cache
        /// </summary>
        /// <param name="p_Key">Key</param>
        public static void ClearCache(string p_Key)
        {
            if (string.IsNullOrEmpty(p_Key))
                return;

            DeleteCacheFile(p_Key);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Write cache text file
        /// </summary>
        /// <param name="p_FileName">Cache ID</param>
        /// <param name="p_Content">Content to write</param>
        /// <returns></returns>
        private static void WriteCacheTextFile(string p_FileName, BeatMaps.MapDetail p_Content)
        {
            if (!m_CacheEnabled)
                return;

            CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() =>
            {
                try
                {
                    var l_JSON = JsonConvert.SerializeObject(p_Content);

                    File.WriteAllText(m_CacheFolder + p_FileName, l_JSON, Encoding.UTF8);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient.WriteCacheTextFile] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                }
            });
        }
        /// <summary>
        /// Write cache file
        /// </summary>
        /// <param name="p_FileName">Cache ID</param>
        /// <param name="p_Content">Content to write</param>
        /// <returns></returns>
        private static void WriteCacheFile(string p_FileName, byte[] p_Content)
        {
            if (!m_CacheEnabled)
                return;

            CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() =>
            {
                try
                {
                    File.WriteAllBytes(m_CacheFolder + p_FileName, p_Content);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient.WriteCacheFile] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                }
            });
        }
        /// <summary>
        /// Delete cache file coroutine
        /// </summary>
        /// <param name="p_Key">File to delete</param>
        /// <returns></returns>
        private static void DeleteCacheFile(string p_Key)
        {
            if (string.IsNullOrEmpty(p_Key))
                return;

            if (!m_CacheEnabled)
                return;

            CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() =>
            {
                try
                {
                    if (File.Exists(m_CacheFolder + p_Key + ".jpg"))
                        File.Delete(m_CacheFolder + p_Key + ".jpg");

                    if (File.Exists(m_CacheFolder + p_Key + ".json"))
                        File.Delete(m_CacheFolder + p_Key + ".json");
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient.DeleteCacheFile] Error :");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Download a song
        /// </summary>
        /// <param name="p_Song">Beat map</param>
        /// <param name="p_Version">Version</param>
        /// <param name="p_Token">Cancellation token</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_Progress">Progress reporter</param>
        /// <returns></returns>
        public static void DownloadSong(BeatMaps.MapDetail p_Song, BeatMaps.MapVersion p_Version, CancellationToken p_Token, Action<bool, string> p_Callback, IProgress<float> p_Progress = null)
        {
            p_Version.ZipBytes(p_Token, (p_Result) =>
            {
                if (p_Result == null || p_Token.IsCancellationRequested)
                {
                    p_Callback?.Invoke(false, string.Empty);
                    return;
                }

                try
                {
                    string l_CustomSongsPath = CustomLevelPathHelper.customLevelsDirectoryPath;

                    if (!Directory.Exists(l_CustomSongsPath))
                        Directory.CreateDirectory(l_CustomSongsPath);

                    CP_SDK.ChatPlexSDK.Logger.Info("[CP_SDK_BS.Game][BeatMapsClient] Downloaded zip!");

                    if (p_Token.IsCancellationRequested)
                    {
                        p_Callback?.Invoke(false, string.Empty);
                        return;
                    }

                    var l_ExtractResult = ExtractZip(p_Song, p_Result, l_CustomSongsPath);
                    p_Callback?.Invoke(l_ExtractResult.Item1, l_ExtractResult.Item2);
                }
                catch (Exception p_Exception)
                {
                    if (p_Exception is TaskCanceledException)
                    {
                        CP_SDK.ChatPlexSDK.Logger.Warning("[CP_SDK_BS.Game][BeatMapsClient] Song Download Aborted.");
                        throw p_Exception;
                    }
                    else
                        CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient] Failed to download Song!");
                }
            }, true, p_Progress);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Extract ZIP archive
        /// </summary>
        /// <param name="p_Song">Beat map</param>
        /// <param name="p_ZIPBytes">Raw ZIP bytes</param>
        /// <param name="p_CustomSongsPath">Extract path</param>
        /// <param name="p_Overwrite">Should overwrite ?</param>
        /// <returns></returns>
        private static (bool, string) ExtractZip(BeatMaps.MapDetail p_Song, byte[] p_ZIPBytes, string p_CustomSongsPath, bool p_Overwrite = false)
        {
            Stream l_ZIPStream = new MemoryStream(p_ZIPBytes);
            l_ZIPStream.Position = 0;

            try
            {
                CP_SDK.ChatPlexSDK.Logger.Info("[CP_SDK_BS.Game][BeatMapsClient] Extracting...");

                /// Prepare base path
                string l_BasePath = p_Song.id + " (" + p_Song.metadata.songName + " - " + p_Song.metadata.levelAuthorName + ")";
                l_BasePath = string.Join("", l_BasePath.Split((Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray())));

                /// Build out path
                string l_OutPath = p_CustomSongsPath + "/" + l_BasePath;

                /// Check to avoid overwrite
                if (!p_Overwrite && Directory.Exists(l_OutPath))
                {
                    int l_FolderCount = 1;

                    while (Directory.Exists(l_OutPath + $" ({l_FolderCount})"))
                        ++l_FolderCount;

                    l_OutPath += $" ({l_FolderCount})";
                    l_BasePath += $" ({l_FolderCount})";
                }

                /// Create directory if needed
                if (!Directory.Exists(l_OutPath))
                    Directory.CreateDirectory(l_OutPath);

                CP_SDK.ChatPlexSDK.Logger.Info("[CP_SDK_BS.Game][BeatMapsClient] " + l_OutPath);

                new FastZip().ExtractZip(l_ZIPStream, l_OutPath, FastZip.Overwrite.Always, null, null, null, true, false, false);

                l_ZIPStream.Close();
                l_ZIPStream = null;

                return (true, l_BasePath);
            }
            catch (Exception p_Exception)
            {
                l_ZIPStream.Close();
                l_ZIPStream = null;

                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient] Unable to extract ZIP! Exception");
                CP_SDK.ChatPlexSDK.Logger.Error(p_Exception);
            }
            finally
            {
                if (l_ZIPStream != null)
                    l_ZIPStream.Close();
            }

            return (false, "");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Object from serialized JSON
        /// </summary>
        /// <typeparam name="t_Type">Object type</typeparam>
        /// <param name="p_Serialized">Input</param>
        /// <param name="p_Object">Result object</param>
        /// <returns>Object or null</returns>
        private static bool GetObjectFromJsonString<t_Type>(string p_Serialized, out t_Type p_Object)
            where t_Type : class, new()
        {
            p_Object = null;
            try
            {
                p_Object = JsonConvert.DeserializeObject<t_Type>(p_Serialized);
            }
            catch (Exception l_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.Game][BeatMapsClient.GetObjectFromJsonString] Error :");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                return false;
            }

            return p_Object != null;
        }
    }
}
