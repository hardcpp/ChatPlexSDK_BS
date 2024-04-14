using IPA.Utilities;
using System.Linq;
using UnityEngine;

namespace CP_SDK_BS.UI
{
    /// <summary>
    /// Vertical icon segmented control
    /// </summary>
    public static class HMUIIconSegmentedControl
    {
        private static HMUI.IconSegmentedControl m_Template;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create icon segmented control
        /// </summary>
        /// <param name="p_Parent">Parent game object transform</param>
        /// <param name="p_HideCellBackground">Should hide cell background</param>
        /// <returns>GameObject</returns>
        public static HMUI.IconSegmentedControl Create(RectTransform p_Parent, bool p_HideCellBackground)
        {
            if (!m_Template)
                m_Template = Resources.FindObjectsOfTypeAll<HMUI.IconSegmentedControl>().First(x => x.name == "BeatmapCharacteristicSegmentedControl" && x._container != null);

            var l_Control = GameObject.Instantiate(m_Template, p_Parent, false);
            l_Control.name = "BSPIconSegmentedControl";
#if BEATSABER_1_35_0_OR_NEWER
            l_Control.SetField<HMUI.SegmentedControl, Zenject.DiContainer>("_container", m_Template._container);
#else
            l_Control.SetField("_container", m_Template._container);
#endif
            l_Control._hideCellBackground =  p_HideCellBackground;

            var l_RectTransform = l_Control.transform as RectTransform;
            l_RectTransform.anchorMin           = Vector2.one * 0.5f;
            l_RectTransform.anchorMax           = Vector2.one * 0.5f;
            l_RectTransform.anchoredPosition    = Vector2.zero;
            l_RectTransform.pivot               = Vector2.one * 0.5f;

            foreach (Transform l_Transform in l_Control.transform)
                GameObject.Destroy(l_Transform.gameObject);

            GameObject.Destroy(l_Control.GetComponent<BeatmapCharacteristicSegmentedControlController>());

            return l_Control;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set data and remove hover hints
        /// </summary>
        /// <param name="p_Instance">Control instance</param>
        /// <param name="p_Data">Data to set</param>
        public static void SetDataNoHoverHint(this HMUI.IconSegmentedControl p_Instance, HMUI.IconSegmentedControl.DataItem[] p_Data)
        {
            try
            {
                p_Instance.SetData(p_Data);

                var l_HoverHints        = p_Instance.GetComponentsInChildren<HMUI.HoverHint>(true);
                var l_LocalHoverHints   = p_Instance.GetComponentsInChildren<LocalizedHoverHint>(true);

                foreach (var l_Current in l_HoverHints) GameObject.Destroy(l_Current);
                foreach (var l_Current in l_LocalHoverHints) GameObject.Destroy(l_Current);
            }
            catch (System.Exception)
            {

            }
        }
    }
}
