using Framework.Extensions;
using HarmonyLib;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.ImpostorRoles.BomberRole.Patches;

[HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
public static class DeadBody_OnClick_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(DeadBody __instance)
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.IsThere() && player.GetRoleManager().IsThere() && player.GetRoleManager().MyRole is Bomber bomber)
            {
                if (bomber.CannotReport.Contains(__instance.ParentId)) return false;
            }
        }

        return true;
    }
}