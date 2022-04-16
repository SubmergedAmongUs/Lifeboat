using System.Linq;
using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.Roles;
using Lifeboat.Roles.ImpostorRoles.AssassinRole;

namespace Lifeboat.RoleAbilities.GuessAbility.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.Assassin_Kill:
                BaseRole role = __instance.GetRoleManager().MyRole;
                (role.Abilities.OfType<GuessAbility>().FirstOrDefault() ?? new GuessAbility(role)).AssassinKill(reader.ReadByte());
                return;
        }
    }
}