using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.CrewmateRoles.OracleRole.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.Oracle_Predict:
                Oracle oracle = (Oracle) __instance.GetRoleManager().MyRole;
                oracle.Predicted = reader.ReadByte();
                oracle.AlignmentOffset = reader.ReadByte();
                return;
        }
    }
}