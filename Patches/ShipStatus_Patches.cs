using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using HarmonyLib;
using Hazel;
using Lifeboat.Debugging;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.GameOptions;
using Lifeboat.RoleModifiers.Modifiers.LoversModifier;
using Lifeboat.Roles;
using Lifeboat.Roles.CrewmateRoles;
using Lifeboat.Roles.CrewmateRoles.AltruistRole;
using Lifeboat.Roles.CrewmateRoles.SwapperRole;
using Lifeboat.Roles.ImpostorRoles;
using Lifeboat.Roles.ImpostorRoles.AssassinRole;
using Lifeboat.Roles.NeutralRoles.GlitchRole;
using Lifeboat.Roles.NeutralRoles.LawyerRole;
using Lifeboat.WinScreen;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Lifeboat.Patches;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.SelectInfected))]
public static class ShipStatus_SelectInfected_Patch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        List<GameData.PlayerInfo> remainingPlayers = GameData.Instance.AllPlayers.ToArray().Where(p => p is {Disconnected: false, IsDead: false}).ToList();
        Random.seed = HashRandom.FastNext(int.MaxValue);

        remainingPlayers.ShuffleList();
        int adjustedNumImpostors = PlayerControl.GameOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount);
            
        List<Type> crewmateRoles = BaseRole.AlignmentToRoleTypes[Alignment.Crewmate].Where(Check).ToList().ShuffleList();
        List<Type> impostorRoles = BaseRole.AlignmentToRoleTypes[Alignment.Impostor].Where(Check).ToList().ShuffleList();
        List<Type> neutralRoles = BaseRole.AlignmentToRoleTypes[Alignment.Neutral].Where(Check).ToList().ShuffleList();

        if (GeneralOptions.AllImpsCanAssassinate) impostorRoles.Remove(typeof(Assassin));

        List<Type> crewRolesOrdered = new();
        foreach (Type role in crewmateRoles)
        {
            if ((float) BaseRole.RoleTypeToAmountField[role].GetValue(null) > 99) crewRolesOrdered.Insert(0, role);
            else crewRolesOrdered.Add(role);
        }
        crewmateRoles = crewRolesOrdered.Take((int) GeneralOptions.CrewmateRoles).ToList();
            
        List<Type> impRolesOrdered = new();
        foreach (Type role in impostorRoles)
        {
            if ((float) BaseRole.RoleTypeToAmountField[role].GetValue(null) > 99) impRolesOrdered.Insert(0, role);
            else impRolesOrdered.Add(role);
        }
        impostorRoles = impRolesOrdered.Take((int) GeneralOptions.ImpostorRoles).ToList();
            
        List<Type> neutralRolesOrdered = new();
        foreach (Type role in neutralRoles)
        {
            if ((float) BaseRole.RoleTypeToAmountField[role].GetValue(null) > 99) neutralRolesOrdered.Insert(0, role);
            else neutralRolesOrdered.Add(role);
        }
        neutralRoles = neutralRolesOrdered.Take((int) GeneralOptions.NeutralRoles).ToList();

        List<(byte playerId, string stringID)> roleData = new();

        for (int i = 0; i < adjustedNumImpostors; i++)
        {
            roleData.Add((remainingPlayers.RemoveAndGet(0).PlayerId, impostorRoles.Any() 
                ? BaseRole.RoleTypeToStringID[impostorRoles.RemoveAndGet(0)] : nameof(StringNames.Impostor)));
        }

        while (remainingPlayers.Any() && neutralRoles.Any())
        {
            roleData.Add((remainingPlayers.RemoveAndGet(0).PlayerId, BaseRole.RoleTypeToStringID[neutralRoles.RemoveAndGet(0)]));
        }

        foreach (GameData.PlayerInfo player in remainingPlayers)
        {
            roleData.Add((player.PlayerId, crewmateRoles.Any() 
                ? BaseRole.RoleTypeToStringID[crewmateRoles.RemoveAndGet(0)] : nameof(StringNames.Crewmate)));
        }

        if (LifeboatDebug.Instance.ForceRolesEnabled)
        {
            List<Type> Roles = BaseRole.RoleTypeToStringID.OrderBy(t => t.Value).Select(s => s.Key).ToList();

            roleData.Clear();
            foreach ((byte p, int value) in LifeboatDebug.Instance.RoleData)
            {
                roleData.Add((p, BaseRole.RoleTypeToStringID[Roles[value]]));
            }
        }

        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.General_SetRoles, SendOption.Reliable);
        writer.Write((byte) roleData.Count);
        foreach ((byte playerId, string stringID) in roleData)
        {
            writer.Write(playerId);
            writer.Write(stringID);
        }

        List<byte> lovers = new();
        if (GeneralOptions.EnableModifiers)
        {
            List<GameData.PlayerInfo> modifiablePlayers = GameData.Instance.AllPlayers.ToArray().Where(p => p is {Disconnected: false, IsDead: false}).ToList();
            modifiablePlayers.RemoveAll(p => BaseRole.StringIDToRoleType[roleData.First(k => k.playerId == p.PlayerId).stringID] == typeof(Swapper));
            modifiablePlayers.ShuffleList();

            if (Random.Range(0, 100) < Lovers.LoversAmount && modifiablePlayers.Count >= 2)
            {
                GameData.PlayerInfo player1, player2;
                try
                {
                    player1 = modifiablePlayers.RemoveAndGet(modifiablePlayers.RandomIdx());

                    switch (Lovers.ChaoticLoversChance)
                    {
                        case 1 or 2:
                            Type role = BaseRole.StringIDToRoleType[roleData.First(k => k.playerId == player1.PlayerId).stringID];

                            if (Lovers.ChaoticLoversChance == 2 || Random.Range(0, 100) < 85)
                            {
                                if (BaseRole.AlignmentToRoleTypes[Alignment.Crewmate].Contains(role))
                                {
                                    List<GameData.PlayerInfo> newPlayers = modifiablePlayers.Where(p => !BaseRole.AlignmentToRoleTypes[Alignment.Crewmate]
                                        .Contains(BaseRole.StringIDToRoleType[roleData.First(k => k.playerId == p.PlayerId).stringID])).ToList();

                                    if (newPlayers.Count > 0) modifiablePlayers = newPlayers;
                                }
                                else if (BaseRole.AlignmentToRoleTypes[Alignment.Impostor].Contains(role))
                                {
                                    List<GameData.PlayerInfo> newPlayers = modifiablePlayers.Where(p => !BaseRole.AlignmentToRoleTypes[Alignment.Impostor]
                                        .Contains(BaseRole.StringIDToRoleType[roleData.First(k => k.playerId == p.PlayerId).stringID])).ToList();

                                    if (newPlayers.Count > 0) modifiablePlayers = newPlayers;
                                }
                            }
                            else if (Lovers.ChaoticLoversChance == 1)
                            {
                                if (BaseRole.AlignmentToRoleTypes[Alignment.Crewmate].Contains(role))
                                {
                                    List<GameData.PlayerInfo> newPlayers = modifiablePlayers.Where(p => BaseRole.AlignmentToRoleTypes[Alignment.Crewmate]
                                        .Contains(BaseRole.StringIDToRoleType[roleData.First(k => k.playerId == p.PlayerId).stringID])).ToList();

                                    if (newPlayers.Count > 0) modifiablePlayers = newPlayers;
                                }
                                else if (BaseRole.AlignmentToRoleTypes[Alignment.Impostor].Contains(role))
                                {
                                    List<GameData.PlayerInfo> newPlayers = modifiablePlayers.Where(p => BaseRole.AlignmentToRoleTypes[Alignment.Impostor]
                                        .Contains(BaseRole.StringIDToRoleType[roleData.First(k => k.playerId == p.PlayerId).stringID])).ToList();

                                    if (newPlayers.Count > 0) modifiablePlayers = newPlayers;
                                }
                            }

                            goto default;

                        default:
                            player2 = modifiablePlayers.RemoveAndGet(modifiablePlayers.RandomIdx());
                            break;
                    }
                }
                catch (Exception e)
                {
                    LifeboatPlugin.Log.LogError(e);
                    writer.Write(0);

                    goto bye;
                }

                writer.Write(1);
                writer.Write(player1.PlayerId);
                writer.Write(player2.PlayerId);
                    
                lovers.Add(player1.PlayerId);
                lovers.Add(player2.PlayerId);
            }
            else
            {
                writer.Write(0);
            }
        }
            
        bye:
            
        writer.EndMessage();

        PlayerControl.LocalPlayer.SetInfected(roleData
            .Where(i => i.stringID != nameof(StringNames.Crewmate) && 
                        (i.stringID == nameof(StringNames.Impostor) || BaseRole.AlignmentToRoleTypes[Alignment.Impostor]
                            .Contains(BaseRole.StringIDToRoleType[i.stringID]))).Select(i => i.playerId).ToArray());
        PlayerControl.LocalPlayer.GetRoleManager().SetRoles(roleData);

        if (lovers.Count == 2)
        {
            Lovers.SetLovers(lovers[0], lovers[1]);
        }
            
        return false;
    }

    public static bool Check(this Type type)
    {
        try
        {
            if (type == typeof(Impostor) || type == typeof(Crewmate)) return false;
            if (!BaseRole.RoleTypeToAmountField.TryGetValue(type, out FieldInfo field)) return false;
            return Random.Range(0, 100) < (float) field.GetValue(null);
        }
        catch (Exception e)
        {
            LifeboatPlugin.Log.LogError(type.Name + ": " + e);
            return false;
        }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
public static class ShipStatus_CheckEndCriteria_Patch
{
    public static uint Timeout;
        
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    public static void Prefix(ShipStatus __instance, out bool __runOriginal)
    {
        if (TutorialManager.InstanceExists)
        {
            __runOriginal = true;
            return;
        }
            
        __runOriginal = false;

        if (Timeout > 0)
        {
            Timeout--;
            return;
        }

        Timeout = 5;
            
        if (LifeboatDebug.Instance.DisableEndGame || !GameData.Instance.IsThere()) return;
            
        List<GameData.PlayerInfo> AllPlayers = GameData.Instance.AllPlayers.ToArray().ToList();
        List<GameData.PlayerInfo> alivePlayers = AllPlayers.Where(p => !p.IsDead && !p.Disconnected).ToList();
        List<GameData.PlayerInfo> aliveImpostors = alivePlayers.Where(p => p.IsImpostor).ToList();

        List<GameData.PlayerInfo> aliveCrewmates = alivePlayers.Where(p => p.Object.IsThere())
            .Where(p => p.Object.GetRoleManager().MyRole.Alignment == Alignment.Crewmate).ToList();
        List<Lovers> aliveLovers = alivePlayers.Where(p => p.Object.IsThere())
            .Select(p => p.Object.GetRoleManager().MyModifier).OfType<Lovers>().ToList();

        if (alivePlayers.Count <= 3 && aliveLovers.Count == 2)
        {
            aliveLovers.First().LoversWin();
            __instance.enabled = false;
            return;
        }

        bool crewTaskWin = GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks && GameData.Instance.TotalTasks > 0;
        if (crewTaskWin && aliveCrewmates.Count > 0)
        {
            __instance.enabled = false;
            RpcCrewWin(nameof(English.Lifeboat_WinReason_TasksCompleted), Palette.CrewmateBlue.ToRGBAString());
            return;
        }

        #region Sabotages

        if (__instance.Systems.ContainsKey(SystemTypes.LifeSupp))
        {
            LifeSuppSystemType lifeSuppSystemType = __instance.Systems[SystemTypes.LifeSupp].Cast<LifeSuppSystemType>();
            if (lifeSuppSystemType.Countdown < 0f)
            {
                RpcImpostorWin(nameof(English.Lifeboat_WinReason_Sabotage), Palette.ImpostorRed.ToRGBAString());
                __instance.enabled = false;
                lifeSuppSystemType.Countdown = 10000f;
                return;
            }
        }

        if (__instance.Systems.ContainsKey(SystemTypes.Reactor))
        {
            ICriticalSabotage criticalSabotage = __instance.Systems[SystemTypes.Reactor].Cast<ICriticalSabotage>();
            if (criticalSabotage.Countdown < 0f)
            {
                RpcImpostorWin(nameof(English.Lifeboat_WinReason_Sabotage), Palette.ImpostorRed.ToRGBAString());
                __instance.enabled = false;
                criticalSabotage.ClearSabotage();
                return;
            }
        }
            
        if (__instance.Systems.ContainsKey(SystemTypes.Laboratory))
        {
            ICriticalSabotage criticalSabotage = __instance.Systems[SystemTypes.Laboratory].Cast<ICriticalSabotage>();
            if (criticalSabotage.Countdown < 0f)
            {
                RpcImpostorWin(nameof(English.Lifeboat_WinReason_Sabotage), Palette.ImpostorRed.ToRGBAString());
                __instance.enabled = false;
                criticalSabotage.ClearSabotage();
                return;
            }
        }

        #endregion
            
        List<BaseRole> aliveGlitches = alivePlayers.Where(p => p.Object.IsThere())
            .Select(p => p.Object.GetRoleManager().MyRole).Where(r => r is Glitch).ToList();
        List<BaseRole> aliveCrewmatesPlusLawyer = alivePlayers.Where(p => p.Object.IsThere()).Select(p => p.Object.GetRoleManager().MyRole)
            .Where(r => r.Alignment == Alignment.Crewmate || r is Lawyer {TargetAlignment: Alignment.Crewmate}).ToList();            
        List<BaseRole> aliveJoustingCrewRoles = AllPlayers.Where(p => !p.IsDead && !p.Disconnected).Where(p => p.Object.IsThere())
            .Select(p => p.Object.GetRoleManager().MyRole).Where(r => r is Altruist or {Owner: {Data: {IsDead: false, Disconnected: false}}})
            .Where(r => r.IsJousting()).ToList();

        if (new[] {aliveImpostors.Count, aliveGlitches.Count, aliveJoustingCrewRoles.Count}.Count(c => c > 0) > 1) return;
            
        if (aliveGlitches.Count == 1 && alivePlayers.Count <= 2)
        {
            ((Glitch) aliveGlitches.First()).GlitchWin();
            __instance.enabled = false;
            return;
        }

        if (aliveImpostors.Count * 2 >= alivePlayers.Count)
        {
            __instance.enabled = false;
            RpcImpostorWin(nameof(English.Lifeboat_WinReason_ImpostorMajority), Palette.ImpostorRed.ToRGBAString());
            return;
        }

        if (aliveImpostors.Count == 0 && aliveGlitches.Count == 0)
        {
            __instance.enabled = false;

            if (aliveCrewmatesPlusLawyer.Count > 0) RpcCrewWin("");
            else RpcStalemate(nameof(LanguageProvider.Current.Lifeboat_WinReason_Stalemate), "808080");
        }
    }
        
    public static void RpcCrewWin(string subtitleStringID, params string[] args)
    {
        TempWinData winData = new()
        {
            SubtitleStringID = subtitleStringID,
            Args = args,
            ShowNames = false,
            WinnerBackgroundBarColor = Palette.CrewmateBlue,
            LoserBackgroundBarColor = Palette.ImpostorRed,
            AudioStinger = TempWinData.Stinger.Crewmate,
            WinnerIds = PlayerControl.AllPlayerControls.ToArray().Where(p => p.GetRoleManager().MyRole.Alignment == Alignment.Crewmate).Select(p => p.PlayerId).ToArray()
        };

        WinScreenNetworking.RpcCustomEndGame(winData);
    }
        
    public static void RpcImpostorWin(string subtitleStringID, params string[] args)
    {
        TempWinData winData = new()
        {
            SubtitleStringID = subtitleStringID,
            Args = args,
            ShowNames = true,
            WinnerBackgroundBarColor = Palette.CrewmateBlue,
            LoserBackgroundBarColor = Palette.ImpostorRed,
            AudioStinger = TempWinData.Stinger.Impostor,
            WinnerIds = GameData.Instance.AllPlayers.ToArray().Where(d => d.IsImpostor).Select(s => s.PlayerId).ToArray()
        };

        WinScreenNetworking.RpcCustomEndGame(winData);
    }
        
    public static void RpcStalemate(string subtitleStringID, params string[] args)
    {
        TempWinData winData = new()
        {
            SubtitleStringID = subtitleStringID,
            Args = args,
            ShowNames = false,
            WinnerBackgroundBarColor = new Color32(128, 128, 128, 255),
            LoserBackgroundBarColor = new Color32(128, 128, 128, 255),
            AudioStinger = TempWinData.Stinger.Disconnect,
            WinnerIds = Array.Empty<byte>(),
        };

        WinScreenNetworking.RpcCustomEndGame(winData);
    }
}
    
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
public static class ShipStatus_IsGameOverDueToDeath_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(out bool __result)
    {
        return __result = false;
    }
}
    
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckTaskCompletion))]
public static class ShipStatus_CheckTaskCompletion_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(out bool __result)
    {
        return __result = false;
    }
}