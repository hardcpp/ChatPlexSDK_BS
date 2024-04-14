using CP_SDK.UI;
using CP_SDK.UI.Components;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace CP_SDK_BS.UI
{
    /// <summary>
    /// Song detail widget
    /// </summary>
    public class LevelDetail
    {
        private static GameObject m_SongDetailViewTemplate = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        internal static void Init()
        {
            if (m_SongDetailViewTemplate)
                return;

            m_SongDetailViewTemplate = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().First(x => x.gameObject.name == "LevelDetail").gameObject);
            m_SongDetailViewTemplate.name = "BSP_SongDetailViewTemplate";

#if BEATSABER_1_35_0_OR_NEWER
            try
            {
                var l_Component = m_SongDetailViewTemplate.GetComponent<StandardLevelDetailView>();
                if (l_Component)
                {
                    var l_Loader = new BeatmapLevelLoader(null, new MockBeatmapDataAssetFileModel(), null, null, new BeatmapLevelLoader.InitData(0));
                    var l_Packs = new List<PackDefinitionSO>();
                    l_Component.SetField("_beatmapLevelsModel", new BeatmapLevelsModel(null, l_Loader, null, l_Packs));
                    GameObject.DestroyImmediate(l_Component);
                }
            }
            catch (Exception l_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.UI][LevelDetail.Init] Error:");
                CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
            }
#else
            GameObject.DestroyImmediate(m_SongDetailViewTemplate.GetComponent<StandardLevelDetailView>());
#endif
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private GameObject                                      m_GameObject;
        private TextMeshProUGUI                                 m_SongNameText;
        private TextMeshProUGUI                                 m_AuthorNameText;
        private HMUI.ImageView                                  m_SongCoverImage;
        private TextMeshProUGUI                                 m_SongTimeText;
        private TextMeshProUGUI                                 m_SongBPMText;
        private TextMeshProUGUI                                 m_SongNPSText;
        private TextMeshProUGUI                                 m_SongNJSText;
        private TextMeshProUGUI                                 m_SongOffsetText;
        private TextMeshProUGUI                                 m_SongNotesText;
        private TextMeshProUGUI                                 m_SongObstaclesText;
        private TextMeshProUGUI                                 m_SongBombsText;
        private BeatmapDifficultySegmentedControlController     m_DifficultiesSegmentedControllerClone;
        private BeatmapCharacteristicSegmentedControlController m_CharacteristicSegmentedControllerClone;
        private HMUI.TextSegmentedControl                       m_SongDiffSegmentedControl;
        private HMUI.IconSegmentedControl                       m_SongCharacteristicSegmentedControl;
        private CSecondaryButton                                m_SecondaryButton                           = null;
        private CPrimaryButton                                  m_PrimaryButton                             = null;
        private GameObject                                      m_FavoriteToggle                            = null;
#if BEATSABER_1_35_0_OR_NEWER
        private BeatmapLevel                                    m_LocalBeatMap                              = null;
#else
        private CustomPreviewBeatmapLevel                       m_LocalBeatMap                              = null;
#endif
        private Game.BeatMaps.MapDetail                         m_BeatMap                                   = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private double  m_Time = 0;
        private float   m_BPM = 0;
        private float   m_NPS = 0;
        private int     m_NJS = 0;
        private float   m_Offset = 0;
        private int     m_Notes = 0;
        private int     m_Obstacles = 0;
        private int     m_Bombs = 0;
        private string  m_Difficulty = "";

        private HMUI.IconSegmentedControl.DataItem m_Characteristic = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public BeatmapCharacteristicSO  SelectedBeatmapCharacteristicSO = null;
        public BeatmapDifficulty        SelectedBeatmapDifficulty       = BeatmapDifficulty.Easy;

#if BEATSABER_1_35_0_OR_NEWER
        public event Action<BeatmapKey> OnActiveDifficultyChanged;
#else
        public event Action<IDifficultyBeatmap> OnActiveDifficultyChanged;
#endif

        public Action OnSecondaryButton;
        public Action OnPrimaryButton;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public string Name {
            get => m_SongNameText.text;
            set {
                var l_HTMLStripped = Regex.Replace(value, "<.*?>", String.Empty);

                if (l_HTMLStripped.Length < 30)
                    m_SongNameText.text = value;
                else
                    m_SongNameText.text = value.Substring(0, 27) + "...";
            }
        }
        public string AuthorNameText
        {
            get => m_AuthorNameText.text;
            set {
                var l_HTMLStripped = Regex.Replace(value, "<.*?>", String.Empty);

                if (l_HTMLStripped.Length < 32)
                    m_AuthorNameText.text = value;
                else
                    m_AuthorNameText.text = value.Substring(0, 29) + "...";
            }
        }
        public Sprite Cover {
            get => m_SongCoverImage.sprite;
            set => m_SongCoverImage.sprite = value;
        }
        public double Time {
            get => m_Time;
            set {
                m_Time = value;
                m_SongTimeText.text = value >= 0.0 ? $"{Math.Floor(value / 60):N0}:{Math.Floor(value % 60):00}" : "--";
            }
        }
        public float BPM {
            get => m_BPM;
            set {
                m_BPM = value;
                m_SongBPMText.text = value.ToString("F0");
            }
        }
        public float NPS {
            get => m_NPS;
            set {
                m_NPS = value;
                m_SongNPSText.text = value >= 0f ? value.ToString("F2") : "--";
            }
        }
        public int NJS {
            get => m_NJS;
            set {
                m_NJS = value;
                m_SongNJSText.text = value >= 0 ? value.ToString() : "--";
            }
        }
        public float Offset {
            get => m_Offset;
            set {
                m_Offset = value;
                m_SongOffsetText.text = !float.IsNaN(value) ? value.ToString("F1") : "--";
            }
        }
        public int Notes {
            get => m_Notes;
            set {
                m_Notes = value;
                m_SongNotesText.text = value >= 0 ? value.ToString() : "--";
            }
        }
        public int Obstacles {
            get => m_Obstacles;
            set {
                m_Obstacles = value;
                m_SongObstaclesText.text = value >= 0 ? value.ToString() : "--";
            }
        }
        public int Bombs {
            get => m_Bombs;
            set {
                m_Bombs = value;
                m_SongBombsText.text = value >= 0 ? value.ToString() : "--";
            }
        }
        public HMUI.IconSegmentedControl.DataItem Characteristic {
            get => m_Characteristic;
            set {
                m_Characteristic = value;
                m_SongCharacteristicSegmentedControl.SetDataNoHoverHint(new List<HMUI.IconSegmentedControl.DataItem>() {
                    value
                }.ToArray());
            }
        }
        public string Difficulty {
            get => m_Difficulty;
            set
            {
                m_Difficulty = value;
                m_SongDiffSegmentedControl.SetTexts(new string[] {
                    value
                });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public LevelDetail(Transform p_Parent)
        {
            m_GameObject = GameObject.Instantiate(m_SongDetailViewTemplate, p_Parent);

            var l_BSMLObjects     = m_GameObject.GetComponentsInChildren<RectTransform>().Where(x => x.gameObject.name.StartsWith("BSML"));
            var l_HoverHints      = m_GameObject.GetComponentsInChildren<HMUI.HoverHint>(true);
            var l_LocalHoverHints = m_GameObject.GetComponentsInChildren<LocalizedHoverHint>(true);

            foreach (var l_Current in l_BSMLObjects)        GameObject.Destroy(l_Current.gameObject);
            foreach (var l_Current in l_HoverHints)         GameObject.Destroy(l_Current);
            foreach (var l_Current in l_LocalHoverHints)    GameObject.Destroy(l_Current);

            /// Favorite toggle
            m_FavoriteToggle = m_GameObject.transform.Find("FavoriteToggle").gameObject;
            m_FavoriteToggle.SetActive(false);

            /// Find play buttons
            var l_ActionButtons     = m_GameObject.transform.Find("ActionButtons");
            var l_PracticeButton    = l_ActionButtons.Find("PracticeButton");
            var l_PlayButton        = l_ActionButtons.Find("ActionButton");

            /// Re-bind play button
            if (l_PlayButton.GetComponent<UnityEngine.UI.Button>())
            {
                var l_ActionButtonsRTransform = l_ActionButtons.transform as RectTransform;
                l_ActionButtonsRTransform.anchoredPosition = new Vector2(-0.5f, l_ActionButtonsRTransform.anchoredPosition.y);

                var l_ButtonsParent = l_PlayButton.transform.parent;
                GameObject.Destroy(l_PracticeButton.gameObject);
                GameObject.Destroy(l_PlayButton.gameObject);

                m_SecondaryButton = UISystem.SecondaryButtonFactory.Create("Secondary", l_ButtonsParent);
                m_SecondaryButton.SetText("Secondary");
                m_SecondaryButton.SetHeight(8f).SetWidth(30f);
                m_SecondaryButton.OnClick(OnSecondaryButtonClicked);

                m_PrimaryButton = UISystem.PrimaryButtonFactory.Create("Primary", l_ButtonsParent);
                m_PrimaryButton.SetText("Primary");
                m_PrimaryButton.SetHeight(8f).SetWidth(30f);
                m_PrimaryButton.OnClick(OnPrimaryButtonClicked);

                SetSecondaryButtonEnabled(false);
                SetSecondaryButtonText("?");
                SetPrimaryButtonEnabled(true);
                SetPrimaryButtonText("?");
            }

            m_CharacteristicSegmentedControllerClone    = m_GameObject.transform.Find("BeatmapCharacteristic").Find("BeatmapCharacteristicSegmentedControl").GetComponent<BeatmapCharacteristicSegmentedControlController>();
            m_SongCharacteristicSegmentedControl        = HMUIIconSegmentedControl.Create(m_CharacteristicSegmentedControllerClone.transform as RectTransform, true);

            m_DifficultiesSegmentedControllerClone  = m_GameObject.transform.Find("BeatmapDifficulty").GetComponentInChildren<BeatmapDifficultySegmentedControlController>();
            m_SongDiffSegmentedControl              = HMUITextSegmentedControl.Create(m_DifficultiesSegmentedControllerClone.transform as RectTransform, true);

            var l_LevelBarBig = m_GameObject.transform.Find("LevelBarBig");

            m_SongNameText      = l_LevelBarBig.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.name == "SongNameText");
            m_AuthorNameText    = l_LevelBarBig.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.name == "AuthorNameText");
            m_SongCoverImage    = l_LevelBarBig.Find("SongArtwork").GetComponent<HMUI.ImageView>();

            m_SongCoverImage.rectTransform.anchoredPosition = new Vector2( 2.000f, m_SongCoverImage.rectTransform.anchoredPosition.y);
            m_SongNameText.rectTransform.anchoredPosition   = new Vector2(-0.195f, m_SongNameText.rectTransform.anchoredPosition.y);
            m_AuthorNameText.richText = true;

            /// Disable multiline
            l_LevelBarBig.Find("MultipleLineTextContainer").gameObject.SetActive(false);

            var l_BeatmapParamsPanel = m_GameObject.transform.Find("BeatmapParamsPanel");
            l_BeatmapParamsPanel.transform.localPosition = l_BeatmapParamsPanel.transform.localPosition + (2 * Vector3.up);

            l_BeatmapParamsPanel.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>().childControlHeight=false;
            l_BeatmapParamsPanel.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();

            m_SongNPSText       = l_BeatmapParamsPanel.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.transform.parent.name == "NPS");
            m_SongNotesText     = l_BeatmapParamsPanel.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.transform.parent.name == "NotesCount");
            m_SongObstaclesText = l_BeatmapParamsPanel.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.transform.parent.name == "ObstaclesCount");
            m_SongBombsText     = l_BeatmapParamsPanel.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.gameObject.transform.parent.name == "BombsCount");

            var l_SizeDelta = (m_SongNPSText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta;
            l_SizeDelta.y *= 2;

            m_SongNPSText.transform.parent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 3);
            m_SongNPSText.transform.parent.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            (m_SongNPSText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta = l_SizeDelta;

            m_SongNotesText.transform.parent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 3);
            m_SongNotesText.transform.parent.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            (m_SongNotesText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta = l_SizeDelta;

            m_SongObstaclesText.transform.parent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 3);
            m_SongObstaclesText.transform.parent.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            (m_SongObstaclesText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta = l_SizeDelta;

            m_SongBombsText.transform.parent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 3);
            m_SongBombsText.transform.parent.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            (m_SongBombsText.transform.parent.transform as UnityEngine.RectTransform).sizeDelta = l_SizeDelta;

            /// Patch
            var l_OffsetTexture = CP_SDK.Unity.Texture2DU.CreateFromRaw(CP_SDK.Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK_BS.UI.Resources.Offset.png"));
            var l_OffsetSprite = CP_SDK.Unity.SpriteU.CreateFromTexture(l_OffsetTexture, 100f, Vector2.one * 16f);
            m_SongOffsetText = GameObject.Instantiate(m_SongNPSText.transform.parent.gameObject, m_SongNPSText.transform.parent.parent).GetComponentInChildren<TextMeshProUGUI>();
            m_SongOffsetText.transform.parent.SetAsFirstSibling();
            m_SongOffsetText.transform.parent.GetComponentInChildren<HMUI.ImageView>().sprite = l_OffsetSprite;

            var l_NJSTexture = CP_SDK.Unity.Texture2DU.CreateFromRaw(CP_SDK.Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK_BS.UI.Resources.NJS.png"));
            var l_NJSSprite = CP_SDK.Unity.SpriteU.CreateFromTexture(l_NJSTexture, 100f, Vector2.one * 16f);
            m_SongNJSText = GameObject.Instantiate(m_SongNPSText.transform.parent.gameObject, m_SongNPSText.transform.parent.parent).GetComponentInChildren<TextMeshProUGUI>();
            m_SongNJSText.transform.parent.SetAsFirstSibling();
            m_SongNJSText.transform.parent.GetComponentInChildren<HMUI.ImageView>().sprite = l_NJSSprite;

            m_SongNPSText.transform.parent.SetAsFirstSibling();

            m_SongBPMText = GameObject.Instantiate(m_SongNPSText.transform.parent.gameObject, m_SongNPSText.transform.parent.parent).GetComponentInChildren<TextMeshProUGUI>();
            m_SongBPMText.transform.parent.SetAsFirstSibling();
            m_SongBPMText.transform.parent.GetComponentInChildren<HMUI.ImageView>().sprite = Resources.FindObjectsOfTypeAll<Sprite>().First(x => x.name == "MetronomeIcon");

            m_SongTimeText = GameObject.Instantiate(m_SongNPSText.transform.parent.gameObject, m_SongNPSText.transform.parent.parent).GetComponentInChildren<TextMeshProUGUI>();
            m_SongTimeText.transform.parent.SetAsFirstSibling();
            m_SongTimeText.transform.parent.GetComponentInChildren<HMUI.ImageView>().sprite = Resources.FindObjectsOfTypeAll<Sprite>().First(x => x.name == "ClockIcon");

            /// Bind events
            m_SongCharacteristicSegmentedControl.didSelectCellEvent += OnCharacteristicChanged;
            m_SongDiffSegmentedControl.didSelectCellEvent           += OnDifficultyChanged;

            try
            {
                foreach (var l_Text in m_GameObject.GetComponentsInChildren<TextMeshProUGUI>(true))
                    l_Text.fontStyle &= ~FontStyles.Italic;

                foreach (var l_Image in m_GameObject.GetComponentsInChildren<HMUI.ImageView>(true))
                {
                    m_SongCoverImage._skew = 0f;
                    m_SongCoverImage.SetAllDirty();
                }
            }
            catch (System.Exception)
            {

            }

            m_GameObject.SetActive(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if the game object is active
        /// </summary>
        /// <param name="p_Active">New state</param>
        public void SetActive(bool p_Active)
        {
            m_GameObject.SetActive(p_Active);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set from game
        /// </summary>
        /// <param name="p_BeatMap">BeatMap</param>
        /// <param name="p_Cover">Cover texture</param>
        /// <param name="p_Characteristic">Game mode</param>
        /// <param name="p_Difficulty">Difficulty</param>
        /// <returns></returns>
#if BEATSABER_1_35_0_OR_NEWER
        public bool FromGame(BeatmapLevel p_BeatMap, Sprite p_Cover, BeatmapCharacteristicSO p_Characteristic, BeatmapDifficulty p_Difficulty)
#else
        public bool FromGame(IBeatmapLevel p_BeatMap, Sprite p_Cover, BeatmapCharacteristicSO p_Characteristic, BeatmapDifficulty p_Difficulty)
#endif
        {
            m_LocalBeatMap      = null;
            m_BeatMap           = null;

            if (p_BeatMap == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.UI][LevelDetail.FromGame] Null Beatmap provided!");
                return false;
            }

            /// Display mode
#if BEATSABER_1_35_0_OR_NEWER
            Characteristic = new HMUI.IconSegmentedControl.DataItem(p_Characteristic.icon, BGLib.Polyglot.Localization.Get(p_Characteristic.descriptionLocalizationKey));

            var l_DifficultyBeatmap = p_BeatMap.GetDifficultyBeatmapData(p_Characteristic, p_Difficulty);
#else
            Characteristic = new HMUI.IconSegmentedControl.DataItem(p_Characteristic.icon, Polyglot.Localization.Get(p_Characteristic.descriptionLocalizationKey));

            var l_IDifficultyBeatmap = p_BeatMap.beatmapLevelData.GetDifficultyBeatmap(p_Characteristic, p_Difficulty);
#endif

            /// Display difficulty
            Difficulty = Game.Levels.BeatmapDifficultySerializedNameToDifficultyName(p_Difficulty.ToString());

            Name            = p_BeatMap.songName;
#if BEATSABER_1_35_0_OR_NEWER
            AuthorNameText  = "Mapped by <b><u>" + (p_BeatMap.allMappers.FirstOrDefault() ?? "") + "</b></u>";
#else
            AuthorNameText  = "Mapped by <b><u>" + p_BeatMap.levelAuthorName + "</b></u>";
#endif
            Cover           = p_Cover ?? Game.Levels.GetDefaultPackCover();
            Time            = p_BeatMap.songDuration;
            BPM             = p_BeatMap.beatsPerMinute;
#if BEATSABER_1_35_0_OR_NEWER
            NJS             = (int)l_DifficultyBeatmap.noteJumpMovementSpeed;
            Offset          = l_DifficultyBeatmap.noteJumpStartBeatOffset;
            NPS             = ((float)l_DifficultyBeatmap.notesCount / (float)p_BeatMap.songDuration);
            Notes           = l_DifficultyBeatmap.notesCount;
            Obstacles       = l_DifficultyBeatmap.obstaclesCount;
            Bombs           = l_DifficultyBeatmap.bombsCount;
#else
            NJS             = (int)l_IDifficultyBeatmap.noteJumpMovementSpeed;
            Offset          = l_IDifficultyBeatmap.noteJumpStartBeatOffset;

            if (l_IDifficultyBeatmap is BeatmapLevelSO.DifficultyBeatmap l_DifficultyBeatmap)
            {
                try
                {
                    /* var l_Task = l_DifficultyBeatmap.GetBeatmapDataBasicInfoAsync();
                    l_Task.ConfigureAwait(false);
                    l_Task.Wait();
                   var l_Info = l_Task.Result;
                    l_DifficultyBeatmap.beatmapLevelData.
                    NPS         = ((float)l_Info.cuttableNotesCount / (float)p_BeatMap.beatsPerMinute);
                    Notes       = l_Info.cuttableNotesCount;
                    Obstacles   = l_Info.obstaclesCount;
                    Bombs       = l_Info.bombsCount;*/
                }
                catch
                {
                    NPS         = -1;
                    Notes       = -1;
                    Obstacles   = -1;
                    Bombs       = -1;
                }
            }
            else if (l_IDifficultyBeatmap is CustomDifficultyBeatmap l_CustomDifficultyBeatmap)
            {
                try
                {
                    NPS         = ((float)l_CustomDifficultyBeatmap.beatmapDataBasicInfo.cuttableNotesCount / (float)p_BeatMap.songDuration);
                    Notes       = l_CustomDifficultyBeatmap.beatmapDataBasicInfo.cuttableNotesCount;
                    Obstacles   = l_CustomDifficultyBeatmap.beatmapDataBasicInfo.obstaclesCount;
                    Bombs       = l_CustomDifficultyBeatmap.beatmapDataBasicInfo.bombsCount;
                }
                catch
                {
                    NPS         = -1;
                    Notes       = -1;
                    Obstacles   = -1;
                    Bombs       = -1;
                }
            }
#endif

            return true;
        }
        /// <summary>
        /// Set from game
        /// </summary>
        /// <param name="p_BeatMap">BeatMap</param>
        /// <param name="p_Cover">Cover texture</param>
        /// <returns></returns>
        public bool FromGame(BeatmapLevel p_BeatMap, Sprite p_Cover)
        {
            m_LocalBeatMap = null;
            m_BeatMap = null;

            if (p_BeatMap == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.UI][LevelDetail.FromGame] Null Beatmap provided!");
                return false;
            }

            /// Display modes
            var l_Characteristics = new List<HMUI.IconSegmentedControl.DataItem>();
#if BEATSABER_1_35_0_OR_NEWER
            foreach (var l_Current in p_BeatMap.GetCharacteristics().Distinct())
                l_Characteristics.Add(new HMUI.IconSegmentedControl.DataItem(l_Current.icon, BGLib.Polyglot.Localization.Get(l_Current.descriptionLocalizationKey)));
#else
            foreach (var l_Current in p_BeatMap.previewDifficultyBeatmapSets.Select(x => x.beatmapCharacteristic).Distinct())
                l_Characteristics.Add(new HMUI.IconSegmentedControl.DataItem(l_Current.icon, Polyglot.Localization.Get(l_Current.descriptionLocalizationKey)));
#endif

            if (l_Characteristics.Count == 0)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.UI][LevelDetail.FromGame] No valid characteristics found for map \"{p_BeatMap.levelID}\"!");
                return false;
            }

            /// Store beatmap
            m_LocalBeatMap = p_BeatMap;

            m_SongCharacteristicSegmentedControl.SetDataNoHoverHint(l_Characteristics.ToArray());
            m_SongCharacteristicSegmentedControl.SelectCellWithNumber(0);
            OnCharacteristicChanged(null, 0);

            /// Display informations
            Name            = p_BeatMap.songName;
#if BEATSABER_1_35_0_OR_NEWER
            AuthorNameText  = "Mapped by <b><u>" + p_BeatMap.allMappers.FirstOrDefault() + "</b></u>";
            Cover           = p_Cover ?? Game.Levels.GetDefaultPackCover();
            BPM             = p_BeatMap.beatsPerMinute;
#else
            AuthorNameText  = "Mapped by <b><u>" + p_BeatMap.levelAuthorName + "</b></u>";
            Cover           = p_Cover ?? Game.Levels.GetDefaultPackCover();
            BPM             = p_BeatMap.standardLevelInfoSaveData.beatsPerMinute;
#endif

            return true;
        }
        /// <summary>
        /// Set from BeatSaver
        /// </summary>
        /// <param name="p_BeatMap">BeatMap</param>
        /// <param name="p_Cover">Cover texture</param>
        /// <param name="p_Characteristic">Game mode</param>
        /// <param name="p_DifficultyRaw">Difficulty raw</param>
        /// <param name="p_CharacteristicSO">Out SO characteristic</param>
        /// <returns></returns>
        public bool FromBeatSaver(Game.BeatMaps.MapDetail p_BeatMap, Sprite p_Cover, string p_Characteristic, string p_DifficultyRaw, out BeatmapCharacteristicSO p_CharacteristicSO)
        {
            m_LocalBeatMap = null;
            m_BeatMap = null;
            p_CharacteristicSO = null;

            if (p_BeatMap == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.UI][LevelDetail.FromBeatSaver1] Null Beatmap provided!");
                return false;
            }

            var l_Version = p_BeatMap.SelectMapVersion();
            if (l_Version == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.UI][LevelDetail.FromBeatSaver1] Null version provided!");
                return false;
            }

            var l_Difficulties = l_Version.GetDifficultiesPerBeatmapCharacteristicSOSerializedName(p_Characteristic);
            if (l_Difficulties.Count == 0)
                return false;

            /// Display mode
            if (!Game.Levels.TryGetBeatmapCharacteristicSOBySerializedName(p_Characteristic, out var l_CharacteristicDetails))
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.UI][LevelDetail.FromBeatSaver1] Characteristic \"{p_Characteristic}\" not found in song core");
                return false;
            }

#if BEATSABER_1_35_0_OR_NEWER
            Characteristic      = new HMUI.IconSegmentedControl.DataItem(l_CharacteristicDetails.icon, BGLib.Polyglot.Localization.Get(l_CharacteristicDetails.descriptionLocalizationKey));
#else
            Characteristic      = new HMUI.IconSegmentedControl.DataItem(l_CharacteristicDetails.icon, Polyglot.Localization.Get(l_CharacteristicDetails.descriptionLocalizationKey));
#endif
            p_CharacteristicSO  = l_CharacteristicDetails;

            /// Select difficulty
            Game.BeatMaps.MapDifficulty l_SelectedDifficulty = null;

            foreach (var l_Current in l_Difficulties)
            {
                if (l_Current.difficulty.ToLower() != p_DifficultyRaw.ToLower())
                    continue;

                l_SelectedDifficulty    = l_Current;
                break;
            }

            if (l_SelectedDifficulty == null)
                return false;

            /// Display difficulty
            Difficulty = Game.Levels.BeatmapDifficultySerializedNameToDifficultyName(p_DifficultyRaw);

            /// Display informations
            Name            = p_BeatMap.metadata.songName;
            AuthorNameText  = "Mapped by <b><u>" + p_BeatMap.metadata.levelAuthorName + "</b></u>";
            Cover           = p_Cover ?? Game.Levels.GetDefaultPackCover();
            Time            = (double)p_BeatMap.metadata.duration;
            BPM             = p_BeatMap.metadata.bpm;
            NPS             = l_SelectedDifficulty.nps;
            NJS             = (int)l_SelectedDifficulty.njs;
            Offset          = l_SelectedDifficulty.offset;
            Notes           = l_SelectedDifficulty.notes;
            Obstacles       = l_SelectedDifficulty.obstacles;
            Bombs           = l_SelectedDifficulty.bombs;

            return true;
        }
        /// <summary>
        /// Set from BeatSaver
        /// </summary>
        /// <param name="p_BeatMap">BeatMap</param>
        /// <param name="p_Cover">Cover texture</param>
        /// <returns></returns>
        public bool FromBeatSaver(Game.BeatMaps.MapDetail p_BeatMap, Sprite p_Cover)
        {
            m_LocalBeatMap = null;
            m_BeatMap = null;

            if (p_BeatMap == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.UI][LevelDetail.FromBeatSaver2] Null Beatmap provided!");
                return false;
            }

            var l_Version = p_BeatMap.SelectMapVersion();
            if (l_Version == null)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.UI][LevelDetail.FromBeatSaver2] Invalid version!");
                return false;
            }

            /// Display modes
            var l_Characteristics = new List<HMUI.IconSegmentedControl.DataItem>();
            foreach (var l_Current in l_Version.GetBeatmapCharacteristicSOSerializedNamesInOrder())
            {
                if (Game.Levels.TryGetBeatmapCharacteristicSOBySerializedName(l_Current, out var l_BeatmapCharacteristicSO))
                {
#if BEATSABER_1_35_0_OR_NEWER
                    l_Characteristics.Add(new HMUI.IconSegmentedControl.DataItem(l_BeatmapCharacteristicSO.icon, BGLib.Polyglot.Localization.Get(l_BeatmapCharacteristicSO.descriptionLocalizationKey)));
#else
                    l_Characteristics.Add(new HMUI.IconSegmentedControl.DataItem(l_BeatmapCharacteristicSO.icon, Polyglot.Localization.Get(l_BeatmapCharacteristicSO.descriptionLocalizationKey)));
#endif
                }
            }

            if (l_Characteristics.Count == 0)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[CP_SDK_BS.UI][LevelDetail.FromBeatSaver2] No valid characteristics found for map \"{p_BeatMap.id}\"!");
                return false;
            }

            /// Store beatmap
            m_BeatMap = p_BeatMap;

            m_SongCharacteristicSegmentedControl.SetDataNoHoverHint(l_Characteristics.ToArray());
            m_SongCharacteristicSegmentedControl.SelectCellWithNumber(0);
            OnCharacteristicChanged(null, 0);

            /// Display informations
            Name            = p_BeatMap.metadata.songName;
            AuthorNameText  = "Mapped by <b><u>" + p_BeatMap.metadata.levelAuthorName + "</b></u>";
            Cover           = p_Cover ?? Game.Levels.GetDefaultPackCover();
            BPM             = p_BeatMap.metadata.bpm;

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set favorite toggle enabled
        /// </summary>
        /// <param name="p_Value"></param>
        public void SetFavoriteToggleEnabled(bool p_Value)
        {
            m_FavoriteToggle.SetActive(p_Value);
        }
        /// <summary>
        /// Set favorite toggle images
        /// </summary>
        /// <param name="p_Default">Default image</param>
        /// <param name="p_Enabled">Enable image</param>
        public void SetFavoriteToggleImage(Sprite p_Default, Sprite p_Enabled)
        {
            var l_IVDefault = m_FavoriteToggle.transform.GetChild(0).GetComponent<HMUI.ImageView>();
            var l_IVMarked  = m_FavoriteToggle.transform.GetChild(1).GetComponent<HMUI.ImageView>();

            l_IVDefault.sprite  = p_Default;
            l_IVMarked.sprite   = p_Enabled;
        }
        /// <summary>
        /// Set favorite toggle hover hint
        /// </summary>
        /// <param name="p_Hint">New hint</param>
        public void SetFavoriteToggleHoverHint(string p_Hint)
        {
            var l_HoverHint = m_FavoriteToggle.GetComponent<HMUI.HoverHint>();
            if (l_HoverHint == null || !l_HoverHint)
            {
                l_HoverHint = m_FavoriteToggle.AddComponent<HMUI.HoverHint>();
                l_HoverHint.SetField("_hoverHintController", Resources.FindObjectsOfTypeAll<HMUI.HoverHintController>().First());
            }

            l_HoverHint.text = p_Hint;
        }
        /// <summary>
        /// Set favorite toggle value
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetFavoriteToggleValue(bool p_Value)
        {
            m_FavoriteToggle.GetComponent<HMUI.ToggleWithCallbacks>().isOn = p_Value;
        }
        /// <summary>
        /// Set favorite toggle callback
        /// </summary>
        /// <param name="p_Action">New callback</param>
        public void SetFavoriteToggleCallback(Action p_Action)
        {
            m_FavoriteToggle.GetComponent<HMUI.ToggleWithCallbacks>().stateDidChangeEvent += (HMUI.ToggleWithCallbacks.SelectionState x) => {
                if (x == HMUI.ToggleWithCallbacks.SelectionState.Pressed)
                    p_Action();
            };
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reverse button order
        /// </summary>
        public void ReverseButtonsOrder()
        {
            m_SecondaryButton.transform.SetAsLastSibling();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set button enabled state
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetSecondaryButtonEnabled(bool p_Value)
        {
            m_SecondaryButton.gameObject.SetActive(p_Value);
        }
        /// <summary>
        /// Set button enabled state
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPrimaryButtonEnabled(bool p_Value)
        {
            m_PrimaryButton.gameObject.SetActive(p_Value);
        }
        /// <summary>
        /// Set button enabled interactable
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPracticeButtonInteractable(bool p_Value)
        {
            m_SecondaryButton.SetInteractable(p_Value);
        }
        /// <summary>
        /// Set button enabled interactable
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPrimaryButtonInteractable(bool p_Value)
        {
            m_PrimaryButton.SetInteractable(p_Value);
        }
        /// <summary>
        /// Set button text
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetSecondaryButtonText(string p_Value)
        {
            m_SecondaryButton.SetText(p_Value);
        }
        /// <summary>
        /// Set button text
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetPrimaryButtonText(string p_Value)
        {
            m_PrimaryButton.SetText(p_Value);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the characteristic changed
        /// </summary>
        /// <param name="p_SegmentControl">Control instance</param>
        /// <param name="p_Index">New selected index</param>
        private void OnCharacteristicChanged(HMUI.SegmentedControl p_SegmentControl, int p_Index)
        {
            if (m_LocalBeatMap != null)
            {
#if BEATSABER_1_35_0_OR_NEWER
                var l_Characs = m_LocalBeatMap.GetCharacteristics().Distinct();
#else
                var l_Characs = m_LocalBeatMap.previewDifficultyBeatmapSets.Select(x => x.beatmapCharacteristic).Distinct();
#endif

                if (p_Index > l_Characs.Count())
                    return;

                SelectedBeatmapCharacteristicSO = l_Characs.ElementAt(p_Index);

#if BEATSABER_1_35_0_OR_NEWER
                List<string> l_Difficulties = m_LocalBeatMap.GetBeatmapKeys()
                    .Where(x => x.beatmapCharacteristic == SelectedBeatmapCharacteristicSO)
                    .Select(x => Game.Levels.BeatmapDifficultySerializedNameToDifficultyName(x.difficulty.SerializedName())).ToList();
#else
                List<string> l_Difficulties = m_LocalBeatMap.previewDifficultyBeatmapSets
                    .Where(x => x.beatmapCharacteristic == SelectedBeatmapCharacteristicSO)
                    .FirstOrDefault().beatmapDifficulties.Select(x => x.SerializedName()).ToList();
#endif

                m_SongDiffSegmentedControl.SetTexts(l_Difficulties.ToArray());
                m_SongDiffSegmentedControl.SelectCellWithNumber(l_Difficulties.Count - 1);
                OnDifficultyChanged(null, l_Difficulties.Count - 1);
            }
            else if (m_BeatMap != null)
            {
                var l_Version = m_BeatMap.SelectMapVersion();
                var l_Characs = l_Version.GetBeatmapCharacteristicSOSerializedNamesInOrder();

                if (p_Index > l_Characs.Count)
                    return;

                Game.Levels.TryGetBeatmapCharacteristicSOBySerializedName(Game.Levels.SanitizeBeatmapCharacteristicSOSerializedName(l_Characs[p_Index]), out SelectedBeatmapCharacteristicSO);

                List<string> l_Difficulties = new List<string>();
                foreach (var l_Current in l_Version.GetDifficultiesPerBeatmapCharacteristicSOSerializedName(l_Characs[p_Index]))
                    l_Difficulties.Add(Game.Levels.BeatmapDifficultySerializedNameToDifficultyName(l_Current.difficulty));

                m_SongDiffSegmentedControl.SetTexts(l_Difficulties.ToArray());
                m_SongDiffSegmentedControl.SelectCellWithNumber(l_Difficulties.Count - 1);
                OnDifficultyChanged(null, l_Difficulties.Count - 1);
            }
        }
        /// <summary>
        /// When the difficulty is changed
        /// </summary>
        /// <param name="p_SegmentControl">Control instance</param>
        /// <param name="p_Index">New selected index</param>
        private void OnDifficultyChanged(HMUI.SegmentedControl p_SegmentControl, int p_Index)
        {
            if (m_LocalBeatMap != null)
            {
#if BEATSABER_1_35_0_OR_NEWER
                var l_Characs = m_LocalBeatMap.GetCharacteristics().Distinct();
#else
                var l_Characs = m_LocalBeatMap.previewDifficultyBeatmapSets.Select(x => x.beatmapCharacteristic).Distinct();
#endif

                if (m_SongCharacteristicSegmentedControl.selectedCellNumber > l_Characs.Count())
                    return;

#if BEATSABER_1_35_0_OR_NEWER
                var l_Difficulties = m_LocalBeatMap.GetBeatmapKeys()
                    .Where(x => x.beatmapCharacteristic == SelectedBeatmapCharacteristicSO);

                if (p_Index < 0 || p_Index >= l_Difficulties.Count())
#else
                var l_Difficulties = m_LocalBeatMap.standardLevelInfoSaveData.difficultyBeatmapSets
                    .Where(x => x.beatmapCharacteristicName == SelectedBeatmapCharacteristicSO.serializedName)
                    .SingleOrDefault();
                if (p_Index < 0 || p_Index >= l_Difficulties.difficultyBeatmaps.Length)
#endif
                {
                    Time        = -1f;
                    NPS         = -1f;
                    NJS         = -1;
                    Offset      = float.NaN;
                    Notes       = -1;
                    Obstacles   = -1;
                    Bombs       = -1;
                    return;
                }

#if BEATSABER_1_35_0_OR_NEWER
                var l_BeatmapKey        = l_Difficulties.ElementAt(p_Index);
                var l_DifficultyBeatmap = m_LocalBeatMap.GetDifficultyBeatmapData(l_BeatmapKey.beatmapCharacteristic, l_BeatmapKey.difficulty);

                Time            = m_LocalBeatMap.songDuration;
                NPS             = ((float)l_DifficultyBeatmap.notesCount / (float)m_LocalBeatMap.songDuration);
                NJS             = (int)l_DifficultyBeatmap.noteJumpMovementSpeed;
                Offset          = l_DifficultyBeatmap.noteJumpStartBeatOffset;
                Notes           = l_DifficultyBeatmap.notesCount;
                Obstacles       = l_DifficultyBeatmap.obstaclesCount;
                Bombs           = l_DifficultyBeatmap.bombsCount;

                if (OnActiveDifficultyChanged != null)
                    OnActiveDifficultyChanged.Invoke(l_BeatmapKey);
#else
                var l_DifficultyBeatMap = l_Difficulties.difficultyBeatmaps.ElementAt(p_Index);
                var l_DifficultyPath    = m_LocalBeatMap.customLevelPath + "\\" + l_DifficultyBeatMap.beatmapFilename;
                var l_Loader            = new BeatmapDataLoader();

                try
                {
                    var l_JSON = System.IO.File.ReadAllText(l_DifficultyPath);

                    var l_BeatmapSaveData   = BeatmapSaveDataVersion3.BeatmapSaveData.DeserializeFromJSONString(l_JSON);
                    var l_Info              = BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveData(l_BeatmapSaveData);
                    if (l_Info != null)
                    {
                        Time            = m_LocalBeatMap.songDuration;
                        NPS             = ((float)l_Info.cuttableNotesCount / (float)m_LocalBeatMap.songDuration);
                        NJS             = (int)l_DifficultyBeatMap.noteJumpMovementSpeed;
                        Offset          = l_DifficultyBeatMap.noteJumpStartBeatOffset;
                        Notes           = l_Info.cuttableNotesCount;
                        Obstacles       = l_Info.obstaclesCount;
                        Bombs           = l_Info.bombsCount;

                        if (OnActiveDifficultyChanged != null)
                            OnActiveDifficultyChanged.Invoke(null);
                    }
                }
                catch (Exception p_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[CP_SDK_BS.UI][LevelDetail.OnDifficultyChanged] Error:");
                    CP_SDK.ChatPlexSDK.Logger.Error(p_Exception);

                    Time        = -1f;
                    NPS         = -1f;
                    NJS         = -1;
                    Offset      = float.NaN;
                    Notes       = -1;
                    Obstacles   = -1;
                    Bombs       = -1;
                    return;
                }
#endif
            }
            else if (m_BeatMap != null)
            {
                var l_Version = m_BeatMap.SelectMapVersion();
                var l_Characs = l_Version.GetBeatmapCharacteristicSOSerializedNamesInOrder();

                if (m_SongCharacteristicSegmentedControl.selectedCellNumber > l_Characs.Count)
                    return;

                var l_Difficulties = l_Version.GetDifficultiesPerBeatmapCharacteristicSOSerializedName(l_Characs[m_SongCharacteristicSegmentedControl.selectedCellNumber]);
                if (p_Index < 0 || p_Index >= l_Difficulties.Count)
                {
                    Time        = -1f;
                    NPS         = -1f;
                    NJS         = -1;
                    Offset      = float.NaN;
                    Notes       = -1;
                    Obstacles   = -1;
                    Bombs       = -1;
                    return;
                }

                var l_SelectedBeatmapCharacteristicDifficulty   = l_Difficulties.ElementAt(p_Index);
                SelectedBeatmapDifficulty                        = Game.Levels.BeatmapDifficultySerializedNameToBeatmapDifficulty(l_SelectedBeatmapCharacteristicDifficulty.difficulty);

                /// Display informations
                Time        = (double)m_BeatMap.metadata.duration;
                NPS         = (float)l_SelectedBeatmapCharacteristicDifficulty.nps;
                NJS         = (int)l_SelectedBeatmapCharacteristicDifficulty.njs;
                Offset      = l_SelectedBeatmapCharacteristicDifficulty.offset;
                Notes       = l_SelectedBeatmapCharacteristicDifficulty.notes;
                Obstacles   = l_SelectedBeatmapCharacteristicDifficulty.obstacles;
                Bombs       = l_SelectedBeatmapCharacteristicDifficulty.bombs;

#if BEATSABER_1_35_0_OR_NEWER
                if (OnActiveDifficultyChanged != null && Game.Levels.TryGetLevelIDFromHash(l_Version.hash, out var l_LevelID))
                    OnActiveDifficultyChanged.Invoke(new BeatmapKey(l_LevelID, SelectedBeatmapCharacteristicSO, SelectedBeatmapDifficulty));
#else
                if (OnActiveDifficultyChanged != null)
                    OnActiveDifficultyChanged.Invoke(null);
#endif
            }
        }
        /// <summary>
        /// Secondary button on click
        /// </summary>
        private void OnSecondaryButtonClicked()
            => OnSecondaryButton?.Invoke();
        /// <summary>
        /// Primary button on click
        /// </summary>
        private void OnPrimaryButtonClicked()
            => OnPrimaryButton?.Invoke();
    }
}
