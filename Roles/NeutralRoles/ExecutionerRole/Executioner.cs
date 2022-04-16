using System.Collections.Generic;
using System.Linq;
using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Hazel;
using Lifeboat.Attributes;
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Events;
using Lifeboat.Extensions;
using Lifeboat.Roles.NeutralRoles.JesterRole;
using Lifeboat.Utils;
using Lifeboat.WinScreen;
using UnityEngine;

namespace Lifeboat.Roles.NeutralRoles.ExecutionerRole;

[OptionHeader(nameof(English.Lifeboat_Executioner))]
public sealed class Executioner : BaseRole
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_NeutralRoles), int.MinValue)] [RoleAmount]
    [NumberOption(nameof(English.Lifeboat_Executioner), "Executioner")] 
    public static float ExecutionerAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 19),
        HeaderColor = new Color32(144, 44, 201, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(ExecutionerAmount, Alignment.Neutral),
    };
        
    [ToggleOption("Submerged", "Executioner_Unused")]
    public static bool UnusedOption = false;
    public static bool UnusedOption_GetVisible() => false;
        
    #endregion

    public override string RoleStringID => nameof(English.Lifeboat_Executioner);
    public override Color32 Color => Settings.HeaderColor;
    public override Alignment Alignment => Alignment.Neutral;
        
    public GameData.PlayerInfo Target { get; set; }
    public bool TargetSet { get; set; }
    public List<NameOverride> NameOverrides { get; } = new();

    public override void Start()
    {
        PlayerControlEvents.OnPlayerExile += CheckPlayerEjected;
            
        if (Owner.AmOwner)
        {
            if (!TargetSet)
            {
                PlayerControl[] allPlayers = PlayerControl.AllPlayerControls.ToArray();
                IList<PlayerControl> nonSelfPlayers = allPlayers.Where(p => !p.AmOwner).ToArray().Shuffle();

                Target = nonSelfPlayers.Where(p => p.GetRoleManager().MyRole?.Alignment == Alignment.Crewmate).ToArray().Shuffle().FirstOrDefault()?.Data;
                Target ??= nonSelfPlayers.Random().Data;
                TargetSet = true;
                MessageWriter writer = AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Executioner_SetTarget, SendOption.Reliable);
                writer.Write(Target.PlayerId);
                writer.EndMessage();
            }
        }
            
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            RoleManager manager = playerControl!?.GetRoleManager();
            if (manager)
            {
                NameOverride nameOverride = new(ExecutionerNameOverride, 10);
                manager.NameOverrides.Add(nameOverride);
                NameOverrides.Add(nameOverride);
            }
        }
    }
        
    public override void Update()
    {
        if (!Owner.AmOwner) return;
        if (!TargetSet || !ShipStatus.Instance || !ShipStatus.Instance.enabled || Owner.Data == null || Owner.Data.IsDead) return;

        if (Target == null || Target.IsDead || Target.Disconnected)
        {
            BecomeJester();
            MessageWriter writer = AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Executioner_BecomeJester);
            writer.EndMessage();
        }
    }

    public void BecomeJester()
    {
        Jester jester = new();
        Owner.GetRoleManager().MyRole = jester;
        if (Target != null)
        {
            jester.PreviousRole = $"{PreviousRole}<color=#{Color.ToRGBAString()}>{RoleName}</color> ({Target.PlayerName}) -> ";
        }
        else
        {
            jester.PreviousRole = $"{PreviousRole}<color=#{Color.ToRGBAString()}>{RoleName}</color> -> ";
        }
        jester.Start();

        if (Owner.AmOwner)
        {
            UsefulMethods.ShowTextToast(string.Format(LanguageProvider.Current.Lifeboat_Executioner_Toast_TargetDied, Color.ToRGBAString(),
                jester.Color.ToRGBAString(), jester.RoleName), 2f);

            Owner.GetRoleManager().StartCoroutine(UsefulMethods.CoFlashScreen(new Color32(255, 117, 239, 77)));
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        PlayerControlEvents.OnPlayerExile -= CheckPlayerEjected;
        foreach (NameOverride nameOverride in NameOverrides)
        {
            nameOverride.Dispose();
        }
    }

    public override List<PlayerControl> GetIntroTeam() => new() {PlayerControl.LocalPlayer, Target.Object};
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Executioner_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString(), Target.PlayerName));
    }

    public override string GetImportantTaskText()
    { 
        return base.GetImportantTaskText() + string.Format(LanguageProvider.Current.Lifeboat_Executioner_TaskText, Color.ToRGBAString(), Target.PlayerName);
    }

    public string ExecutionerNameOverride(PlayerControl player, string currentName, bool inMeeting)
    {
        if (!player.IsThere() || Target == null) return currentName;
        if (player.PlayerId != Target.PlayerId) return currentName;
        if (PlayerControl.LocalPlayer.PlayerId != Owner.PlayerId && !RoleManager.SeesRolesAsGhost()) return currentName;

        return $"{currentName}{(currentName.Contains("]") ? "" : " ")}<color=#{Color.ToRGBAString()}>[⦿]</color>";
    }
        
    public void CheckPlayerEjected(PlayerControl player)
    {
        if (player && player.Data != null && Owner && Owner.Data != null && player.Data.PlayerId == Target.PlayerId && !Owner.Data.IsDead)
        {
            ExecutionerWin();
        }
    }
        
    public void ExecutionerWin()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        ShipStatus.Instance.enabled = false;
            
        TempWinData winData = new()
        {
            SubtitleStringID = nameof(English.Lifeboat_WinReason_Executioner), 
            Args = new [] {Owner.Data.PlayerName, Color.ToRGBAString(), Target.PlayerName},
            ShowNames = true,
            WinnerBackgroundBarColor = Color,
            LoserBackgroundBarColor = Color,
            AudioStinger = TempWinData.Stinger.Impostor,
            WinnerIds = new[] {Owner.PlayerId}
        };

        WinScreenNetworking.RpcCustomEndGame(winData);
    }
        
    public override string GetGameSummaryDescription(bool hasWon)
    {
        return $"{(hasWon ? "<color=green>" : "")}{Owner.Data.PlayerName}:{(hasWon ? "</color>" : "")} " +
               $"{PreviousRole}" +
               $"<color=#{Color.ToRGBAString()}>{RoleName}</color> ({Target?.PlayerName})" +
               $"{(Owner.GetRoleManager().MyModifier is { } modifier ? " " + modifier.GetGameSummaryDescription() : "")}";
    }

    public override void Deserialize(MessageReader reader)
    {
        Target = GameData.Instance.GetPlayerById(reader.ReadByte());
        TargetSet = reader.ReadBoolean();
    }

    public override void Serialize(MessageWriter writer)
    {
        writer.Write(Target.PlayerId);
        writer.Write(TargetSet);
    }

    public override BaseRole CreateClone()
    {
        return new Executioner
        {
            Target = Target,
            TargetSet = TargetSet,
        };
    }
}