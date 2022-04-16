using System.Linq;
using HarmonyLib;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.Roles;

namespace Lifeboat.Patches;

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.Detoriorate))]
public static class SabotageSystemType_Detoriorate_Patch
{
    [HarmonyPostfix]
    public static void Postfix(SabotageSystemType __instance)
    {
        if (PlayerControl.AllPlayerControls.ToArray().Where(p => p.GetRoleManager()!?.MyRole.Alignment == Alignment.Impostor)
                .All(p => p.Data.IsDead && !RoleManager.CanBeRevived(p)) && !TutorialManager.InstanceExists)
        {
            __instance.ForceSabTime(0);
            __instance.Timer = 30;
        }
    }
}