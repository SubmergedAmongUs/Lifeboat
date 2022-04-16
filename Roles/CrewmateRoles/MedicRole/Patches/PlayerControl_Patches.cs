using System.Collections.Generic;
using Framework.Extensions;
using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.CrewmateRoles.MedicRole.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControl_MurderPlayer_Patch
{
    public static List<PlayerControl> ShowFlashTo;
        
    [HarmonyPrefix, HarmonyPriority(Priority.HigherThanNormal)]
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        return !Medic.CheckMedicProtection(__instance, target, false, ShowFlashTo);
    }

    [HarmonyPostfix]
    public static void Postfix()
    {
        ShowFlashTo = null;
    }
}
    
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.Medic_Monitor:
                ((Medic) __instance.GetRoleManager().MyRole).Protect(GameData.Instance.GetPlayerById(reader.ReadByte()).Object);
                return;
        }
    }
}