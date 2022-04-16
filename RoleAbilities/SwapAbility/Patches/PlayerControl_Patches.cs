using System.Linq;
using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.Roles;

namespace Lifeboat.RoleAbilities.SwapAbility.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.Swapper_Swap:
                BaseRole role = __instance.GetRoleManager().MyRole;
                int priority = reader.ReadPackedInt32();
                (role.Abilities.OfType<SwapAbility>().FirstOrDefault() ?? new SwapAbility(role, priority)).Swap(reader.ReadByte(), reader.ReadByte());
                return;
        }
    }
}