using HarmonyLib;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.NeutralRoles.GlitchRole.Patches;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
public static class ShipStatus_CalculateLightRadius_Patch
{
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    public static void Prefix(ShipStatus __instance)
    {
        if (PlayerControl.LocalPlayer.GetRoleManager().MyRole is Glitch)
        {
            PlayerControl.LocalPlayer.Data.IsImpostor = true;
        }
    }
        
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void Postfix(ShipStatus __instance)
    {
        if (PlayerControl.LocalPlayer.GetRoleManager().MyRole is Glitch)
        {
            PlayerControl.LocalPlayer.Data.IsImpostor = false;
        }
    }
}