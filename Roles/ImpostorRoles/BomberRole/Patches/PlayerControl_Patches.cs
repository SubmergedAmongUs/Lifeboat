using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.ImpostorRoles.BomberRole.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.Bomber_PlantBomb:
                ((Bomber) __instance.GetRoleManager().MyRole).PlantBomb(GameData.Instance.GetPlayerById(reader.ReadByte()).Object);
                return;
                
            case CustomRpcCalls.Bomber_Explode:
                ((Bomber) GameData.Instance.GetPlayerById(reader.ReadByte()).Object.GetRoleManager().MyRole).Explode(__instance, 
                    GameData.Instance.GetPlayerById(reader.ReadByte()).Object);
                return;
                
            case CustomRpcCalls.Bomber_ExplodeAtMeeting:
                ((Bomber) GameData.Instance.GetPlayerById(reader.ReadByte()).Object.GetRoleManager().MyRole).ExplodeAtMeeting(__instance);
                return;
        }
    }
}