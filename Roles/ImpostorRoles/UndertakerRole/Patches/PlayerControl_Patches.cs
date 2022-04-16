using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.ImpostorRoles.UndertakerRole.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.Undertaker_Drag:
                ((Undertaker) __instance.GetRoleManager().MyRole).Drag(reader.ReadByte());
                return;
                
            case CustomRpcCalls.Undertaker_Drop:
                ((Undertaker) __instance.GetRoleManager().MyRole).Drop(reader.ReadByte());
                return;
        }
    }
}