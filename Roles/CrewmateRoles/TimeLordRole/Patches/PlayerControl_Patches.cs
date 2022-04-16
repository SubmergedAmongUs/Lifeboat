using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.CrewmateRoles.TimeLordRole.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.TimeLord_Rewind:
                RewindTime.StartRewind(__instance.GetRoleManager().MyRole as TimeLord);
                return;
        }
    }
}