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
using Lifeboat.Roles.CrewmateRoles.OracleRole.Buttons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Lifeboat.Roles.CrewmateRoles.OracleRole;

[OptionHeader(nameof(English.Lifeboat_Oracle))]
public sealed class Oracle : Crewmate
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_CrewmateRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Oracle), "Oracle")] 
    public static float OracleAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 95),
        HeaderColor = new Color32(52, 79, 235, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(OracleAmount, Alignment.Crewmate),
    };

    [NumberOption(nameof(English.Lifeboat_Oracle_GameOptions_Accuracy), "Prediction Accuracy", 10)] 
    public static float PredictionAccuracy = 80;

    [NumberOption(nameof(English.Lifeboat_Oracle_GameOptions_Cooldown), "Oracle_Cooldown", 9, 
        5, 30, 2.5f, false, "{0:0.0}s")]
    public static float PredictCooldown = 10;

    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Oracle);
    public override Color32 Color => Settings.HeaderColor;

    public PredictButton PredictButton { get; set; }
    public int Meetings { get; set; }
    public byte Predicted { get; set; } = byte.MaxValue;
    public int AlignmentOffset { get; set; }
    public Color[] AlignmentColors { get; } = 
    {
        Palette.CrewmateBlue,
        Palette.ImpostorRed,
        Palette.Purple,
        Palette.CrewmateBlue,
        Palette.ImpostorRed,
    };
        
    public override void Start()
    {
        MeetingHudEvents.OnMeetingUpdate += HandleMeetingHudUpdate;
            
        if (Owner.AmOwner)
        {
            new PredictButton(this);
        }
            
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            RoleManager manager = playerControl!?.GetRoleManager();
            if (manager) manager.NameOverrides.Add(new NameOverride(OracleNameOverride, 10));
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        MeetingHudEvents.OnMeetingUpdate -= HandleMeetingHudUpdate;
    }

    public void HandleMeetingHudUpdate(MeetingHud meeting)
    {
        if (Predicted == byte.MaxValue || !Owner.Data.IsDead) return;
        if (Owner.GetRoleManager().KilledBy == Owner.PlayerId) return;

        GameData.PlayerInfo predictedInfo = GameData.Instance.GetPlayerById(Predicted);
        if (predictedInfo == null || !predictedInfo.Object) return;

        BaseRole predictedRole = predictedInfo.Object.GetRoleManager().MyRole;
        if (predictedRole == null) return;

        Color changedColor = AlignmentColors[(int) predictedRole.Alignment + AlignmentOffset];
            
        foreach (PlayerVoteArea voteArea in meeting.playerStates)
        {
            voteArea.PlayerButton.GetComponent<SpriteRenderer>().color = voteArea.TargetPlayerId != Predicted ? UnityEngine.Color.white : changedColor;
        }
    }

    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Oracle_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public void RpcPredictPlayer(byte playerId)
    {
        Predicted = playerId;
        AlignmentOffset = Random.Range(0, 100) < PredictionAccuracy ? 0 : Random.Range(1, 3);
            
        MessageWriter writer = AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Oracle_Predict, SendOption.Reliable);
        writer.Write(Predicted);
        writer.Write((byte) AlignmentOffset);
        writer.EndMessage();
    }
        
    public string OracleNameOverride(PlayerControl player, string currentName, bool _)
    {
        if (!player.IsThere()) return currentName;
        if (player.PlayerId != Predicted) return currentName;
        if (PlayerControl.LocalPlayer.PlayerId != Owner.PlayerId && !RoleManager.SeesRolesAsGhost()) return currentName;

        return $"{currentName}{(currentName.Contains("]") ? "" : " ")}<color=#{Color.ToRGBAString()}>[※]</color>";
    }
}