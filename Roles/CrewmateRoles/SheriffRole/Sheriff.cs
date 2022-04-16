using System.Collections.Generic;
using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Lifeboat.Attributes;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.GameOptions;
using Lifeboat.Roles.CrewmateRoles.SheriffRole.Buttons;
using Lifeboat.Roles.NeutralRoles.GlitchRole;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.SheriffRole;

[OptionHeader(nameof(English.Lifeboat_Sheriff))]
public sealed class Sheriff : Crewmate
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_CrewmateRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Sheriff), "Sheriff")] 
    public static float SheriffAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 94),
        HeaderColor = new Color32(204, 172, 55, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(SheriffAmount, Alignment.Crewmate),
    };
        
    [StringOption(nameof(English.Lifeboat_Sheriff_GameOptions_CanKill), "Sheriff_CanKill", 10,
        nameof(English.Lifeboat_GameOptions_Generic_Impostors), nameof(English.Lifeboat_GameOptions_Generic_ImpGlitch),
        nameof(English.Lifeboat_Sheriff_GameOptions_CanKill_ImpNeutral), nameof(English.Lifeboat_Sheriff_GameOptions_CanKill_Everyone))]
    public static int CanKill = 2;
        
    [ToggleOption(nameof(English.Lifeboat_Sheriff_GameOptions_MisfireKillsSheriff), "Misfire Kills Sheriff", 9)] 
    public static bool MisfireKillsSheriff = true;
    public static bool MisfireKillsSheriff_GetVisible => CanKill < 3 || GeneralOptions.ShouldShowMeaningless;
        
    [ToggleOption(nameof(English.Lifeboat_Sheriff_GameOptions_MisfireKillsTarget), "Misfire Kills Target", 8)]
    public static bool MisfireKillsTarget = false;
    public static bool MisfireKillsTarget_GetVisible => CanKill < 3 || GeneralOptions.ShouldShowMeaningless;

    [ToggleOption(nameof(English.Lifeboat_Sheriff_GameOptions_MatchImpostorCooldown), "Sheriff_MatchImpostorCooldown", 6)]
    public static bool MatchImpostorCooldown = true;
        
    [NumberOption(nameof(English.Lifeboat_Sheriff_GameOptions_Cooldown), "Sheriff Kill Cooldown", 5, 
        5, 60, 2.5f, false, "{0:0.0}s")]
    public static float SheriffKillCooldown = 25;
    public static bool SheriffKillCooldown_GetVisible => !MatchImpostorCooldown || GeneralOptions.ShouldShowMeaningless;

    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Sheriff);
    public override Color32 Color => Settings.HeaderColor;

    public override bool IsJousting() => true;

    public List<byte> Killed { get; } = new();
        
    public override void Start()
    {
        if (Owner.AmOwner)
        {
            new SheriffKillButton(this);
        }
    }
        
    public override void Update()
    {
        if (!Owner.AmOwner) return;
        if (Owner.PlayerId != PlayerControl.LocalPlayer.PlayerId) return;
        Vector2 myPosition = PlayerControl.LocalPlayer.GetTruePosition();

        float maxDistance = PlayerControl.LocalPlayer.MaxReportDistance;
        Collider2D closestHit = null;

        foreach (Collider2D body in Physics2D.OverlapCircleAll(myPosition, PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
        {
            if (!body.CompareTag("DeadBody")) continue;
            float distance = Vector2.Distance(body.transform.position, myPosition);
                
            if (distance < maxDistance)
            {
                DeadBody component = body.GetComponent<DeadBody>();
                if (!Killed.Contains(component.ParentId) && component.enabled &&
                    !PhysicsHelpers.AnythingBetween(myPosition, component.TruePosition, Constants.ShipAndObjectsMask, false))
                {
                    maxDistance = distance;
                    closestHit = body;
                }
            }
        }

        HudManager.Instance.ReportButton.SetActive(closestHit);
    }

    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Sheriff_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }
        
    public void MurderPlayer(PlayerControl target)
    {
        BaseRole targetRole = target.GetRoleManager().MyRole;
        Alignment targetAlignment = targetRole.Alignment;

        bool isOk = CanKill switch
        {
            0 => targetAlignment == Alignment.Impostor,
            1 => targetAlignment == Alignment.Impostor || targetRole is Glitch,
            2 => targetAlignment is Alignment.Impostor or Alignment.Neutral, 
            _ => true,
        };
        bool shouldKillTarget = isOk || MisfireKillsTarget;
        bool shouldKillSheriff = !isOk && MisfireKillsSheriff;

        if (shouldKillTarget)
        {
            PlayerControl.LocalPlayer.RpcMurderPlayer(target);
            if (target.gameObject.layer == LayerMask.NameToLayer("Ghost"))
            {
                Killed.Add(target.PlayerId);
            }
        }

        if (shouldKillSheriff)
        {
            PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
            Killed.Add(PlayerControl.LocalPlayer.PlayerId);
        }
    }
}