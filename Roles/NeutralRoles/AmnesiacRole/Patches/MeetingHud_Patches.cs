using Essentials.Common.Extensions;
using Essentials.PluginLoader.Extensions;
using HarmonyLib;
using Mods.Lifeboat.Extensions;
using Mods.Lifeboat.Roles.NeutralRoles.AmnesiacRole.MonoBehaviours;
using UnityEngine;

namespace Mods.Lifeboat.Roles.NeutralRoles.AmnesiacRole.Patches
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
    public static class MeetingHud_PopulateResults_Patch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (!PlayerControl.LocalPlayer.Is(out Amnesiac _)) return;
            
            GameObject.FindObjectsOfType<RememberButton>().ForEach(g => g.gameObject.Destroy());
        }
    }
}