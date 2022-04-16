using HarmonyLib;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.NeutralRoles.GlitchRole.Patches;

[HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
public static class DeadBody_OnClick_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(DeadBody __instance)
    {                
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.GetRoleManager().MyRole is not Glitch glitch) continue;
            if (!glitch.Hacked) continue;                
            if (glitch.Hacked.AmOwner) return false;
        }

        return true;
    }
}