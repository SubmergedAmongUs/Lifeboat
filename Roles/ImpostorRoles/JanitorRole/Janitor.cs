using System.Linq;
using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Lifeboat.Attributes;
using Lifeboat.Enums;
using Lifeboat.Events;
using Lifeboat.Extensions;
using Lifeboat.Roles.ImpostorRoles.JanitorRole.Buttons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Lifeboat.Roles.ImpostorRoles.JanitorRole;

[OptionHeader(nameof(English.Lifeboat_Janitor))]
public sealed class Janitor : Impostor
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_ImpostorRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Janitor), "Janitor")] 
    public static float JanitorAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 56),
        HeaderColor = Palette.ImpostorRed,
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(JanitorAmount, Alignment.Impostor),
    };

    [NumberOption(nameof(English.Lifeboat_Janitor_GameOptions_Cooldown), "Janitor Clean Cooldown", 10,
        5, 60, 2.5f, false, "{0:0.0}s")] 
    public static float CleanCooldown = 25;
        
    [ToggleOption(nameof(English.Lifeboat_Janitor_GameOptions_ResetKillOnClean), "Reset Kill Cooldown On Clean", 9)]
    public static bool ResetOnClean = true;

    [ToggleOption(nameof(English.Lifeboat_Janitor_GameOptions_ResetCleanOnKill), "Reset Clean Cooldown On Kill", 8)]
    public static bool ResetOnKill = true;
        
    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Janitor);

    public CleanButton Button { get; set; }
        
    public override void Start()
    {
        base.Start();
        if (Owner.AmOwner)
        {
            Button = new CleanButton(this);
            PlayerControlEvents.OnPlayerMurder += HandleMurderPlayer;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        PlayerControlEvents.OnPlayerMurder -= HandleMurderPlayer;
    }

    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Janitor_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public void HandleMurderPlayer(PlayerControl player, PlayerControl target)
    {
        if (ResetOnKill && Owner.AmOwner && player.AmOwner && target && target.Data.IsDead)
        {
            Button.CurrentTime = Button.Cooldown;
        }
    }

    public void Clean(byte playerId)
    {
        foreach (DeadBody body in GameObject.FindObjectsOfType<DeadBody>().Where(b => b.ParentId == playerId)) Object.Destroy(body.gameObject);
    }
}