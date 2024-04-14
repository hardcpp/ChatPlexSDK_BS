using HarmonyLib;
using System;
using VRUIControls;

namespace CP_SDK_BS.UI.Patches
{
    [HarmonyPatch(typeof(VRPointer))]
#if BEATSABER_1_35_0_OR_NEWER
    [HarmonyPatch(nameof(VRPointer.EnabledLastSelectedPointer), new Type[] { })]
#else
    [HarmonyPatch(nameof(VRPointer.OnEnable), new Type[] { })]
#endif
    internal class PVRPointer
    {
        internal static event Action<VRPointer> OnActivated;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">VRPointer instance</param>
        internal static void Postfix(VRPointer __instance)
        {
            try { OnActivated?.Invoke(__instance); }
            catch { }
        }
    }
}
