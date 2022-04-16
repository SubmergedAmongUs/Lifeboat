using Framework.Extensions;
using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using UnhollowerBaseLib;

namespace Lifeboat.Roles.ImpostorRoles.GrenadierRole.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.Grenadier_Smoke:
                Grenadier grenadier = (Grenadier) __instance.GetRoleManager().MyRole;
                Il2CppStructArray<byte> playerIds = reader.ReadBytesAndSize();
                if (playerIds.Contains(PlayerControl.LocalPlayer.PlayerId))
                {
                    grenadier.Smoke(PlayerControl.LocalPlayer.GetRoleManager().MyRole.Alignment == Alignment.Impostor || PlayerControl.LocalPlayer.Data.IsDead ? 0.2f : 1f);
                }
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (!player.IsThere() || player.Data.IsDead || player.Data.Disconnected || player.Data.IsImpostor) continue;
                        
                    if (playerIds.Contains(player.PlayerId))
                    {
                        grenadier.Owner.GetRoleManager().StartCoroutine(grenadier.CoDoNameOverride(player));
                    }
                }
                return;
        }
    }
}