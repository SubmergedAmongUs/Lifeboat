using System.Collections.Generic;
using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Lifeboat.Attributes;
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.Roles.NeutralRoles.ArsonistRole.Buttons;
using Lifeboat.WinScreen;
using UnityEngine;

namespace Lifeboat.Roles.NeutralRoles.ArsonistRole;

[OptionHeader(nameof(English.Lifeboat_Arsonist))]
public sealed class Arsonist : BaseRole
{
    #region Options
        
    [OptionHeader(nameof(English.Lifeboat_GameOptions_NeutralRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Arsonist), "Arsonist")] 
    public static float ArsonistAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 20),
        HeaderColor = new Color32(227, 134, 41, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(ArsonistAmount, Alignment.Neutral),
    };
        
    [NumberOption(nameof(English.Lifeboat_Arsonist_GameOptions_Cooldown), "Douse Cooldown", 10,
        5, 60, 2.5f, false, "{0:0.0}s")]
    public static float DouseCooldown = 25;

    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Arsonist);
    public override Color32 Color => Settings.HeaderColor;
    public override Alignment Alignment => Alignment.Neutral;
        
    public List<byte> DousedPlayers { get; } = new();

    public override void Start()
    {
        if (Owner.AmOwner)
        {
            new DouseButton(this);
        }
            
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            RoleManager manager = playerControl!?.GetRoleManager();
            if (manager) manager.NameOverrides.Add(new NameOverride(ArsonistNameOverride, 10));
        }
    }

    public override void Update()
    {
        if (Owner.Data.IsDead && MeetingHud.Instance || Owner.Data.Disconnected) DousedPlayers.Clear();
    }
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Arsonist_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public void ArsonistWin()
    {
        TempWinData winData = new()
        {
            SubtitleStringID = nameof(English.Lifeboat_WinReason_Arsonist),
            Args = new[] {Owner.Data.PlayerName, Color.ToRGBAString()},
            ShowNames = false,
            WinnerBackgroundBarColor = Color,
            LoserBackgroundBarColor = Color,
            AudioStinger = TempWinData.Stinger.Impostor,
            WinnerIds = new[] {Owner.PlayerId}
        };

        WinScreenNetworking.RpcCustomEndGame(winData);
    }
        
    public string ArsonistNameOverride(PlayerControl player, string currentName, bool inMeeting)
    {
        if (!player.IsThere() || DousedPlayers == null) return currentName;
        if (!DousedPlayers.Contains(player.PlayerId)) return currentName;
        if (PlayerControl.LocalPlayer.PlayerId != Owner.PlayerId && !RoleManager.SeesRolesAsGhost()) return currentName;

        return $"{currentName}{(currentName.Contains("]") ? "" : " ")}<color=#{Color.ToRGBAString()}>[♨]</color>";
    }
}