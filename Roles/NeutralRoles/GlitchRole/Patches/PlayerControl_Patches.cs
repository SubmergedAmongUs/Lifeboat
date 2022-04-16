using Framework.Extensions;
using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.NeutralRoles.GlitchRole.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.Glitch_Hack:
                RoleManager roleManager = __instance.GetRoleManager();
                roleManager.StartCoroutine(((Glitch) roleManager.MyRole).CoHackPlayer(reader.ReadByte()));
                return;
                
            case CustomRpcCalls.Glitch_Morph:
                ((Glitch) __instance.GetRoleManager().MyRole).Morph(reader.ReadByte(), reader.ReadSingle());
                return;
        }
    }
}