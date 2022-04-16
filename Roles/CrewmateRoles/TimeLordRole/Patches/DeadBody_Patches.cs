using HarmonyLib;

namespace Lifeboat.Roles.CrewmateRoles.TimeLordRole.Patches;

[HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
public static class DeadBody_OnClick_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(DeadBody __instance)
    {
        return RewindTime.RewindRoutine == null;
    }
}