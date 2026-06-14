using HarmonyLib;
using System;
using VRUIControls;

namespace CP_SDK_BS.UI.Patches
{
    [HarmonyPatch(typeof(VRPointer))]
    [HarmonyPatch(nameof(VRPointer.EnabledLastSelectedPointer), new Type[] { })]
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
