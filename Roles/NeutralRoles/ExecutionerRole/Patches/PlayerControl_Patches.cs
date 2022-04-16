using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.NeutralRoles.ExecutionerRole.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.Executioner_SetTarget:
                Executioner executioner = (Executioner) __instance.GetRoleManager().MyRole;
                executioner.Target = GameData.Instance.GetPlayerById(reader.ReadByte());
                executioner.TargetSet = true;
                return;
                
            case CustomRpcCalls.Executioner_BecomeJester:
                ((Executioner) __instance.GetRoleManager().MyRole).BecomeJester();
                return;
        }
    }
}