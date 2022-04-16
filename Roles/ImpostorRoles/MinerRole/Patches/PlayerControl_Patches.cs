using Framework.Extensions;
using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.ImpostorRoles.MinerRole.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.Miner_PlaceVent:
                ((Miner) __instance.GetRoleManager().MyRole).Place(reader.ReadVector2());
                return;
        }
    }
}