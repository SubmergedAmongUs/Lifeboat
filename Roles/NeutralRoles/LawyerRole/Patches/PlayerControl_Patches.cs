using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.NeutralRoles.LawyerRole.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControl_MurderPlayer_Patch
{
    public static List<PlayerControl> ShowFlashTo;

    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        return !Lawyer.CheckLawyerProtection(__instance, target, false, ShowFlashTo);
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
            case CustomRpcCalls.Lawyer_SetClient:
                Lawyer lawyer = (Lawyer) __instance.GetRoleManager().MyRole;
                lawyer.Target = GameData.Instance.GetPlayerById(reader.ReadByte());
                lawyer.TargetSet = true;
                return;
                
            case CustomRpcCalls.Lawyer_BecomeJester:
                ((Lawyer) __instance.GetRoleManager().MyRole).BecomeJester();
                return;
        }
    }
}