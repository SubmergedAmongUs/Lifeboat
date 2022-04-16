using HarmonyLib;
using Hazel;
using Lifeboat.Enums;

namespace Lifeboat.RoleModifiers.Modifiers.LoversModifier.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.Lovers_SendFreeChat:
                Lovers.AddLoversChat(__instance, reader.ReadString());
                return;
                
            case CustomRpcCalls.Lovers_SendQuickChat:
                Lovers.AddLoversChat(__instance, QuickChatNetData.Deserialize(reader));
                return;
        }
    }
}