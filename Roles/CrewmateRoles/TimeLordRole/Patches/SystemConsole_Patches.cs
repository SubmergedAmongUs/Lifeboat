using HarmonyLib;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.CrewmateRoles.TimeLordRole.Patches;

[HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
public static class SystemConsole_CanUse_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(SystemConsole __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc,
        [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse)
    {
        if (__instance.MinigamePrefab.TryCast<VitalsMinigame>() && PlayerControl.LocalPlayer.GetRoleManager().MyRole is TimeLord && !TimeLord.CanUseVitals)
        {
            return canUse = couldUse = false;
        }

        return true;
    }
}