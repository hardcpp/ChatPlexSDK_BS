﻿using CP_SDK.UI.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CP_SDK_BS.UI.Data
{
    /// <summary>
    /// Song list item
    /// </summary>
    public class SongListItem : IListItem
    {
        private static Sprite                               m_DefaultCover      = null;
        private static Dictionary<string, Sprite>           m_CoverCache        = new Dictionary<string, Sprite>();
        private static Dictionary<string, AudioClip>        m_AudioClipCache    = new Dictionary<string, AudioClip>();
        private static SongPreviewPlayer                    m_SongPreviewPlayer = null;
        private static CP_SDK.Misc.FastCancellationToken    m_LoadAudioToken    = new CP_SDK.Misc.FastCancellationToken();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private bool                        m_WasInit               = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public string                       TitlePrefix         = string.Empty;
        public Game.BeatMaps.MapDetail      BeatSaver_Map       = null;
        public BeatmapLevel                 BeatmapLevel        = null;
        public Sprite                       Cover               = null;
        public string                       Tooltip             = string.Empty;
        public SongListController           SongListController  = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public bool Invalid => (BeatmapLevel == null && BeatSaver_Map == null);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /*
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_TitlePrefix">Title prefix</param>
        /// <param name="p_BeatSaver_Map">Remote map</param>
        /// <param name="p_CustomLevel">Local map</param>
        /// <param name="p_HoverHint">Hover hint text</param>
        /// <param name="p_CustomData">User custom dama</param>
        /// <param name="p_SongListController">Song list controller</param>
        public SongListItem(
                string                      p_TitlePrefix,
                Game.BeatMaps.MapDetail     p_BeatSaver_Map,
                CustomPreviewBeatmapLevel   p_CustomLevel,
                string                      p_HoverHint             = "",
                SongListController          p_SongListController    = null
            )
        {
            TitlePrefix           = p_TitlePrefix;
            BeatSaver_Map         = p_BeatSaver_Map;
            m_CustomLevel           = p_CustomLevel;
            m_HoverHint             = p_HoverHint;
            m_SongListController    = p_SongListController;
        }
        */
        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Init()
        {
            if (m_WasInit)
                return;

            if (!m_DefaultCover)
            {
                var l_GamePrefab = UnityEngine.Resources.FindObjectsOfTypeAll<LevelListTableCell>().FirstOrDefault();
                if (l_GamePrefab)
                    m_DefaultCover = l_GamePrefab._coverImage?.sprite ?? null;
            }

            if (!m_SongPreviewPlayer)
                m_SongPreviewPlayer = UnityEngine.Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().FirstOrDefault();

            if (BeatmapLevel == null && BeatSaver_Map != null && !BeatSaver_Map.Partial)
            {
                if (Game.Levels.TryGetBeatmapLevelForHash(GetLevelHash(), out var l_LocalLevel))
                    BeatmapLevel = l_LocalLevel;
            }

            m_WasInit = true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get level ID
        /// </summary>
        /// <returns></returns>
        public string GetLevelID()
        {
            if (BeatSaver_Map != null
                && BeatSaver_Map.SelectMapVersion() != null
                && BeatSaver_Map.SelectMapVersion().hash != null
                && Game.Levels.TryGetLevelIDFromHash(BeatSaver_Map.SelectMapVersion().hash, out var l_LevelID))
                return l_LevelID;
            else if (BeatmapLevel != null)
                return BeatmapLevel.levelID;

            return "";
        }
        /// <summary>
        /// Get level hash
        /// </summary>
        /// <returns></returns>
        public string GetLevelHash()
        {
            if (BeatSaver_Map != null && BeatSaver_Map.SelectMapVersion() != null && BeatSaver_Map.SelectMapVersion().hash != null)
                return BeatSaver_Map.SelectMapVersion().hash.ToUpper();
            else if (BeatmapLevel != null && Game.Levels.LevelID_IsCustom(BeatmapLevel.levelID) && Game.Levels.TryGetHashFromLevelID(BeatmapLevel.levelID, out var l_Hash))
                return l_Hash;

            return "";
        }
        /// <summary>
        /// Get level author name
        /// </summary>
        /// <returns></returns>
        public string GetLevelAuthorName()
        {
            if (BeatSaver_Map != null && BeatSaver_Map.SelectMapVersion() != null && BeatSaver_Map.SelectMapVersion().hash != null)
                return BeatSaver_Map.metadata.levelAuthorName;
            else if (BeatmapLevel != null)
                return string.Join(", ", BeatmapLevel.allMappers);

            return "<unk>";
        }
        /// <summary>
        /// Get level uploader
        /// </summary>
        /// <returns></returns>
        public string GetLevelUploaderName()
        {
            if (BeatSaver_Map != null && BeatSaver_Map.SelectMapVersion() != null && BeatSaver_Map.SelectMapVersion().hash != null)
                return BeatSaver_Map.uploader.name;
            else if (BeatmapLevel != null)
                return BeatmapLevel.allMappers.FirstOrDefault();

            return "<unk>";
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get song name
        /// </summary>
        /// <returns></returns>
        public string GetSongName()
        {
            if (BeatSaver_Map != null && BeatSaver_Map.SelectMapVersion() != null && BeatSaver_Map.SelectMapVersion().hash != null)
                return BeatSaver_Map.metadata.songName;
            else if (BeatmapLevel != null )
                return BeatmapLevel.songName;

            return "<unk>";
        }
        /// <summary>
        /// Get song author name
        /// </summary>
        /// <returns></returns>
        public string GetSongAuthorName()
        {
            if (BeatSaver_Map != null && BeatSaver_Map.SelectMapVersion() != null && BeatSaver_Map.SelectMapVersion().hash != null)
                return BeatSaver_Map.metadata.songAuthorName;
            else if (BeatmapLevel != null)
                return BeatmapLevel.songAuthorName;

            return "<unk>";
        }
        /// <summary>
        /// Get song duration in seconds
        /// </summary>
        /// <returns></returns>
        public int GetSongDuration()
        {
            if (BeatSaver_Map != null && !BeatSaver_Map.Partial)
                return BeatSaver_Map.metadata.duration;
            else if (BeatmapLevel != null)
                return (int)BeatmapLevel.songDuration;

            return 0;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On show
        /// </summary>
        public override void OnShow()
        {
            if (!(Cell is SongListCell l_SongListCell))
                return;

            Init();

            var l_Title         = "";
            var l_SubTitle      = "";
            var l_Tooltip       = Tooltip == null ? "" : Tooltip;
            var l_BPMText       = "";
            var l_DurationText  = "";

            if ((BeatSaver_Map != null && !BeatSaver_Map.Partial) || BeatmapLevel != null)
            {
                var l_HaveSong  = Game.Levels.TryGetBeatmapLevelForLevelID(GetLevelID(), out _);
                var l_Scores    = Game.Levels.GetScoresByLevelID(GetLevelID(), out var l_HaveAnyScore, out var l_HaveAllScores);

                var l_Duration      = GetSongDuration();
                var l_BPM           = BeatmapLevel != null ? BeatmapLevel.beatsPerMinute                : BeatSaver_Map.metadata.bpm;

                if (l_Scores.Count != 0)
                {
                    foreach (var l_Row in l_Scores)
                    {
                        l_Tooltip += $"\n{l_Row.Key.serializedName} ";
                        foreach (var l_SubRow in l_Row.Value)
                            l_Tooltip += (l_SubRow.Item2 != -1 ? "<color=green>✔</color> " : "<color=red>❌</color> ");
                    }
                }

                var l_TitleBuilder = new StringBuilder(60);
                if (!string.IsNullOrWhiteSpace(TitlePrefix))
                    l_TitleBuilder.Append(TitlePrefix);

                if (BeatSaver_Map?.ranked ?? false)
                    l_TitleBuilder.Append("<#F8E600><b>⭐</b>");

                if (l_HaveAllScores)
                    l_TitleBuilder.Append("<#52F700>");
                else if (l_HaveAnyScore)
                    l_TitleBuilder.Append("<#F8E600>");
                else if (l_HaveSong)
                    l_TitleBuilder.Append("<#CCCCCC>");
                else
                    l_TitleBuilder.Append("<#FFFFFF>");

                l_TitleBuilder.Append(GetSongName());

                l_Title         = l_TitleBuilder.ToString();
                l_SubTitle      = $"{GetSongAuthorName()} <#FFFFFF>[<size=85%>{GetLevelAuthorName()}]";
                l_BPMText       = ((int)l_BPM).ToString();
                l_DurationText  = l_Duration >= 0.0 ? $"{Math.Floor((double)l_Duration / 60):N0}:{Math.Floor((double)l_Duration % 60):00}" : "--";
            }
            else if (BeatSaver_Map != null && BeatSaver_Map.Partial)
                l_Title = "Loading from BeatSaver...";
            else
            {
                l_Title     = "<#FF0000>Invalid song";
                l_SubTitle  = BeatmapLevel != null && Game.Levels.TryGetHashFromLevelID(BeatmapLevel.levelID, out var l_Hash) ? l_Hash : "";
            }

            l_SongListCell.Cover.SetSprite(Cover ?? m_DefaultCover);
            l_SongListCell.Title.SetText(l_Title);
            l_SongListCell.SubTitle.SetText(l_SubTitle);

            if (!string.IsNullOrEmpty(l_DurationText))
            {
                l_SongListCell.Duration.gameObject.SetActive(true);
                l_SongListCell.Duration.SetText(l_DurationText);
            }
            else
                l_SongListCell.Duration.gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(l_BPMText))
            {
                l_SongListCell.BPM.gameObject.SetActive(true);
                l_SongListCell.BPM.SetText(l_BPMText);
            }
            else
                l_SongListCell.BPM.gameObject.SetActive(false);

            l_SongListCell.Tooltip = l_Tooltip;

            if (Cover == null || !Cover)
                LoadLevelCover();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On select
        /// </summary>
        public override void OnSelect()
        {
            var l_PlayPreviewAudio      = SongListController?.PlayPreviewAudio()   ?? false;
            var l_PreviewAudioVolume    = SongListController?.PreviewAudioVolume() ?? 0.5f;

            if (m_SongPreviewPlayer == null || !m_SongPreviewPlayer || !l_PlayPreviewAudio)
                return;

            if (Game.Levels.TryGetBeatmapLevelForLevelID(GetLevelID(), out var l_LocalSong))
            {
                if (m_AudioClipCache.TryGetValue(GetLevelHash(), out var l_AudioClip))
                {
                    if (m_SongPreviewPlayer.activeAudioClip == l_AudioClip)
                        return;

                    m_SongPreviewPlayer.CrossfadeTo(l_AudioClip, l_PreviewAudioVolume, l_LocalSong.previewStartTime, l_LocalSong.previewDuration, () => { });
                }
                else
                {
                    m_LoadAudioToken.Cancel();

                    if (l_LocalSong.previewMediaData is FileSystemPreviewMediaData l_FileSystemPreviewMediaData)
                        CP_SDK.Unity.MTCoroutineStarter.Start(Coroutine_GetAudioAsync(l_FileSystemPreviewMediaData._previewAudioClipPath, l_PreviewAudioVolume));
                }
            }
            else
            {
                /// Stop preview music if any
                m_SongPreviewPlayer.CrossfadeToDefault();
            }
        }
        /// <summary>
        /// On Unselect
        /// </summary>
        public override void OnUnselect()
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Stop preview music if any
        /// </summary>
        public void StopPreviewMusic()
        {
            if (m_SongPreviewPlayer != null && m_SongPreviewPlayer)
                m_SongPreviewPlayer.CrossfadeToDefault();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load level cover
        /// </summary>
        private void LoadLevelCover()
        {
            if (!Cover && m_CoverCache.TryGetValue(GetLevelHash(), out var l_Cover))
            {
                if (l_Cover)
                {
                    CoverLoaded(l_Cover);
                    return;
                }

                m_CoverCache.Remove(GetLevelHash());
            }

            if (Game.Levels.TryGetBeatmapLevelForLevelID(GetLevelID(), out var l_LocalSong))
            {
                Game.Levels.TryLoadBeatmapLevelCoverAsync(l_LocalSong, (_, p_Sprite) => CoverLoaded(p_Sprite));
            }
            else if (BeatSaver_Map != null)
            {
                var l_CoverByte = Game.BeatMapsClient.GetCoverImageFromCacheByKey(BeatSaver_Map.id);
                if (l_CoverByte != null && l_CoverByte.Length > 0)
                {
                    var l_Texture = CP_SDK.Unity.Texture2DU.CreateFromRaw(l_CoverByte);
                    if (l_Texture != null)
                        CoverLoaded(Sprite.Create(l_Texture, new Rect(0, 0, l_Texture.width, l_Texture.height), new Vector2(0.5f, 0.5f), 100));
                }
                else
                {
                    /// Fetch cover
                    BeatSaver_Map.SelectMapVersion().CoverImageBytes((p_Valid, p_CoverTaskResult) =>
                    {
                        if (p_Valid)
                            Game.BeatMapsClient.CacheCoverImage(BeatSaver_Map, p_CoverTaskResult);

                        CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                        {
                            var l_Texture = CP_SDK.Unity.Texture2DU.CreateFromRaw(p_CoverTaskResult);
                            if (l_Texture != null)
                                CoverLoaded(Sprite.Create(l_Texture, new Rect(0, 0, l_Texture.width, l_Texture.height), new Vector2(0.5f, 0.5f), 100));
                        });
                    });
                }
            }
        }
        /// <summary>
        /// Level cover loaded
        /// </summary>
        /// <param name="p_Cover">Loaded cover</param>
        private void CoverLoaded(Sprite p_Cover)
        {
            Cover = p_Cover;

            if (!m_CoverCache.ContainsKey(GetLevelHash()))
                m_CoverCache.Add(GetLevelHash(), p_Cover);

            if ((Cell is SongListCell l_SongListCell))
                l_SongListCell.Cover.SetSprite(Cover ?? m_DefaultCover);

            try
            {
                SongListController?.OnSongListItemCoverFetched(this);
            }
            catch (Exception l_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.UI.Data][SongListItem.CoverLoaded] Error:");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load audio clip
        /// </summary>
        /// <param name="p_Path">Path to load</param>
        /// <param name="p_PreviewAudioVolume">Preview volume</param>
        /// <returns></returns>
        private IEnumerator Coroutine_GetAudioAsync(string p_Path, float p_PreviewAudioVolume)
        {
            var l_StartSerial = m_LoadAudioToken.Serial;

            yield return new WaitForEndOfFrame();

            if (m_LoadAudioToken.IsCancelled(l_StartSerial))
                yield break;

            var l_PathParts = p_Path.Split('/');
            var l_SafePath  = string.Join("/", l_PathParts.Select(x => x == l_PathParts[0] ? x : Uri.EscapeUriString(x)).ToArray());
            var l_FinalURL  = "file://" + l_SafePath.Replace("#", "%23");

            yield return new WaitForEndOfFrame();

            UnityWebRequest l_Loader = UnityWebRequestMultimedia.GetAudioClip(l_FinalURL, AudioType.OGGVORBIS);
            yield return l_Loader.SendWebRequest();

            /// Skip if it's not the menu
            if (CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.EGenericScene.Menu)
                yield break;

            if (m_LoadAudioToken.IsCancelled(l_StartSerial))
                yield break;

            if (l_Loader.isNetworkError
                || l_Loader.isHttpError
                || !string.IsNullOrEmpty(l_Loader.error))
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.UI.Data][SongListItem.Coroutine_GetAudioAsync] Can't load audio! {(!string.IsNullOrEmpty(l_Loader.error) ? l_Loader.error : string.Empty)}");
                yield break;
            }

            var l_AudioClip = null as AudioClip;
            try
            {
                ((DownloadHandlerAudioClip)l_Loader.downloadHandler).streamAudio = true;
                l_AudioClip = DownloadHandlerAudioClip.GetContent(l_Loader);

                if (l_AudioClip == null)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.UI.Data][SongListItem.Coroutine_GetAudioAsync] No audio found");
                    yield break;
                }
            }
            catch (Exception p_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.UI.Data][SongListItem.Coroutine_GetAudioAsync] Can't load audio! Exception:");
                CP_SDK.ChatPlexSDK.Logger.Error(p_Exception);
                yield break;
            }

            var l_RemainingTry  = 15;
            var l_Waiter        = new WaitForSecondsRealtime(0.1f);

            while (l_AudioClip.loadState != AudioDataLoadState.Loaded
                && l_AudioClip.loadState != AudioDataLoadState.Failed)
            {
                yield return l_Waiter;
                l_RemainingTry--;

                if (l_RemainingTry < 0)
                    yield break;

                if (m_LoadAudioToken.IsCancelled(l_StartSerial))
                    yield break;
            }

            if (CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.EGenericScene.Menu)
                yield break;

            if (m_LoadAudioToken.IsCancelled(l_StartSerial))
                yield break;

            if (l_AudioClip.loadState != AudioDataLoadState.Loaded)
                yield break;

            try
            {
                if (!m_AudioClipCache.ContainsKey(GetLevelHash()))
                    m_AudioClipCache.Add(GetLevelHash(), l_AudioClip);

                if (m_SongPreviewPlayer && m_SongPreviewPlayer.activeAudioClip != l_AudioClip)
                    m_SongPreviewPlayer.CrossfadeTo(l_AudioClip, p_PreviewAudioVolume, BeatmapLevel.previewStartTime, BeatmapLevel.previewDuration, () => { });
            }
            catch (Exception)
            {

            }
        }
    }
}
