using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Lifeboat.Attributes;
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Events;
using Lifeboat.Extensions;
using Lifeboat.Roles.NeutralRoles.LawyerRole;
using Lifeboat.WinScreen;
using UnityEngine;

namespace Lifeboat.Roles.NeutralRoles.JesterRole;

[OptionHeader(nameof(English.Lifeboat_Jester))]
public sealed class Jester : BaseRole
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_NeutralRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Jester), "Jester")] 
    public static float JesterAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 17),
        HeaderColor = new Color32(255, 117, 239, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(JesterAmount, Alignment.Neutral),
    };

    [ToggleOption("Submerged", "Jester_Unused")]
    public static bool UnusedOption = false;
    public static bool UnusedOption_GetVisible() => false;

    #endregion

    public override string RoleStringID => nameof(English.Lifeboat_Jester);
    public override Color32 Color => Settings.HeaderColor;
    public override Alignment Alignment => Alignment.Neutral;
        
    public byte LawyerRevealedPlayer { get; set; } = byte.MaxValue;

    public override void Awake() => PlayerControlEvents.OnPlayerExile += CheckPlayerEjected;

    public override void Start()
    {
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            RoleManager manager = playerControl!?.GetRoleManager();
            if (manager) manager.NameOverrides.Add(new NameOverride(JesterLawyerNameOverride, 5));
        }
    }
        
    public override void OnDestroy()
    {
        base.OnDestroy();
        PlayerControlEvents.OnPlayerExile -= CheckPlayerEjected;
    }

    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Jester_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public void CheckPlayerEjected(PlayerControl player)
    {
        if (player == Owner)
        {
            JesterWin();
        }
    }

    public void JesterWin()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (ShipStatus.Instance) ShipStatus.Instance.enabled = false;
            
        TempWinData winData = new()
        {
            SubtitleStringID = nameof(English.Lifeboat_WinReason_Jester),
            Args = new[] {Owner.Data.PlayerName, Color.ToRGBAString()},
            ShowNames = true,
            WinnerBackgroundBarColor = Color,
            LoserBackgroundBarColor = Color,
            AudioStinger = TempWinData.Stinger.Impostor,
            WinnerIds = new[] {Owner.PlayerId}
        };

        WinScreenNetworking.RpcCustomEndGame(winData);
    }
        
    public string JesterLawyerNameOverride(PlayerControl player, string currentName, bool inMeeting)
    {
        if (!player.IsThere()) return currentName;
        if (player.PlayerId != LawyerRevealedPlayer) return currentName;
        if (PlayerControl.LocalPlayer.PlayerId != Owner.PlayerId && !RoleManager.SeesRolesAsGhost()) return currentName;

        BaseRole role = player!.GetRoleManager()?.MyRole;
        if (inMeeting && role != null && !RoleManager.SeesRolesAsGhost())
        {
            return $"<color=#{role.Color.ToRGBAString()}>{currentName}</color> <color=#{Lawyer.Settings.HeaderColor.ToRGBAString()}>[★]</color>\n" +
                   $"<size=70%><color=#{role.Color.ToRGBAString()}>{role.RoleName}";
        }

        return $"{currentName}{(currentName.Contains("]") ? "" : " ")}<color=#{Lawyer.Settings.HeaderColor.ToRGBAString()}>[★]</color>";
    }
}