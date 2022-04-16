using HarmonyLib;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.CrewmateRoles.SheriffRole.Patches;

[HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
public static class DeadBody_OnClick_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(DeadBody __instance)
    {
        if (PlayerControl.LocalPlayer.GetRoleManager().MyRole is not Sheriff sheriff) return true;
        return !sheriff.Killed.Contains(__instance.ParentId);
    }
}