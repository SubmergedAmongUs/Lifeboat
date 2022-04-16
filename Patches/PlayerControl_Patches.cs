using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.RoleModifiers.Modifiers.LoversModifier;
using Lifeboat.Roles;
using Lifeboat.WinScreen;

namespace Lifeboat.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch ((CustomRpcCalls) callId)
        {
            case CustomRpcCalls.General_EndGame:
                if (AmongUsClient.Instance.AmHost) WinScreenNetworking.HandleCustomEndGame(reader);
                return;
                
            case CustomRpcCalls.General_SetRoles:
                byte length = reader.ReadByte();
                List<(byte pid, string stringID)> items = new();
                for (int i = 0; i < length; i++) items.Add((reader.ReadByte(), reader.ReadString()));
                PlayerControl.LocalPlayer.SetInfected(items
                    .Where(i => i.stringID != nameof(StringNames.Crewmate) && 
                                (i.stringID == nameof(StringNames.Impostor) || BaseRole.AlignmentToRoleTypes[Alignment.Impostor]
                                    .Contains(BaseRole.StringIDToRoleType[i.stringID]))).Select(i => i.pid).ToArray());
                PlayerControl.LocalPlayer.GetRoleManager().SetRoles(items);
                if (reader.ReadInt32() == 1)
                {
                    Lovers.SetLovers(reader.ReadByte(), reader.ReadByte());
                }
                return;
        }
    }
}   
    
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Awake))]
public static class PlayerControl_Awake_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance)
    {
        PlayerControlExtensions.RoleManagers[__instance] = __instance.gameObject.AddComponent<RoleManager>();
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnDestroy))]
public static class PlayerControl_OnDestroy_Patch
{
    [HarmonyPrefix]
    public static void Prefix(PlayerControl __instance)
    {
        PlayerControlExtensions.RoleManagers.Remove(__instance);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControl_MurderPlayer_Patch
{
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        target.GetRoleManager().KilledBy = __instance.PlayerId;

        ShipStatus_CheckEndCriteria_Patch.Timeout = 50;
    }
}
    
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
public static class PlayerControl_Exiled_Patch
{
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    public static void Prefix(PlayerControl __instance)
    {
        ShipStatus_CheckEndCriteria_Patch.Timeout = 50;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
public static class PlayerControl_Die_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance)
    {
        if (__instance.AmOwner)
        {
            HudManager.Instance.ReportButton.gameObject.SetActive(false);
            HudManager.Instance.KillButton.gameObject.SetActive(false);
        }
    }
}
    
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
public static class PlayerControl_Revive_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance)
    {
        if (__instance.AmOwner)
        {
            HudManager.Instance.ReportButton.gameObject.SetActive(true);
            HudManager.Instance.KillButton.gameObject.SetActive(__instance.Data.IsImpostor);
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class PlayerControl_SetKillTimer_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
    {
        if (PlayerControl.GameOptions.KillCooldown > 0)
        {
            __instance.killTimer = time;
            HudManager.Instance.KillButton.SetCoolDown(__instance.killTimer, PlayerControl.GameOptions.KillCooldown);
        }

        return false;
    }
}