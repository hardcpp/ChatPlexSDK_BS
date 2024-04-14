using IPA.Utilities;
using System.Linq;
using UnityEngine;

namespace CP_SDK_BS.UI
{
    /// <summary>
    /// Text segmented control
    /// </summary>
    public static class HMUITextSegmentedControl
    {
        private static HMUI.TextSegmentedControl m_Template;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create text segmented control
        /// </summary>
        /// <param name="p_Parent">Parent game object transform</param>
        /// <param name="p_HideCellBackground">Should hide cell background</param>
        /// <param name="p_Texts">Texts</param>
        /// <returns>GameObject</returns>
        public static HMUI.TextSegmentedControl Create(RectTransform p_Parent, bool p_HideCellBackground, string[] p_Texts = null)
        {
            if (!m_Template)
                m_Template = Resources.FindObjectsOfTypeAll<HMUI.TextSegmentedControl>().First(x => x.name == "BeatmapDifficultySegmentedControl" && x._container != null);

            var l_Control = GameObject.Instantiate(m_Template, p_Parent, false);
            l_Control.name = "BSPTextSegmentedControl";
#if BEATSABER_1_35_0_OR_NEWER
            l_Control.SetField<HMUI.SegmentedControl, Zenject.DiContainer>("_container", m_Template._container);
#else
            l_Control.SetField("_container", m_Template._container);
#endif
            l_Control._hideCellBackground = p_HideCellBackground;

            var l_RectTransform = l_Control.transform as RectTransform;
            l_RectTransform.anchorMin           = Vector2.one * 0.5f;
            l_RectTransform.anchorMax           = Vector2.one * 0.5f;
            l_RectTransform.anchoredPosition    = Vector2.zero;
            l_RectTransform.pivot               = Vector2.one * 0.5f;

            foreach (Transform l_Transform in l_Control.transform)
                GameObject.Destroy(l_Transform.gameObject);

            GameObject.Destroy(l_Control.GetComponent<BeatmapDifficultySegmentedControlController>());

            l_Control.SetTexts(p_Texts != null ? p_Texts : new string[] { "Tab" });

            return l_Control;
        }
    }
}
