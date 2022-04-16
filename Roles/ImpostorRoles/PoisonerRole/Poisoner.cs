using System.Collections;
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
using Lifeboat.Events;
using Lifeboat.Extensions;
using Lifeboat.RoleAbilities.GuessAbility;
using Lifeboat.Roles.ImpostorRoles.PoisonerRole.Buttons;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles.PoisonerRole;

[OptionHeader(nameof(English.Lifeboat_Poisoner))]
public sealed class Poisoner : Impostor
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_ImpostorRoles), int.MinValue)] [RoleAmount]
    [NumberOption(nameof(English.Lifeboat_Poisoner), "Poisoner")]
    public static float PoisonerAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 53),
        HeaderColor = new Color32(186, 85, 211, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(PoisonerAmount, Alignment.Impostor),
    };

    [NumberOption(nameof(English.Lifeboat_Poisoner_GameOptions_KillDelay), "Poisoner_KillDelay", 10, 
        5, 40, 1f, false, "{0:0}s")]
    public static float KillDelay = 15;

    #endregion

    public override string RoleStringID => nameof(English.Lifeboat_Poisoner);
    public override Color32 Color => Settings.HeaderColor;

    public PoisonButton Button { get; set; }
    public byte PoisonTarget { get; set; } = byte.MaxValue;

    public override void Start()
    {
        base.Start();
            
        if (Owner.AmOwner)
        {
            Button = new PoisonButton(this);
                
            PlayerControlEvents.OnPlayerMurder += HandlePlayerMurder;
            PlayerControlEvents.OnPlayerDisconnect += HandlePlayerDisconnect;
        }
            
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            RoleManager manager = playerControl!?.GetRoleManager();
            if (manager) manager.NameOverrides.Add(new NameOverride(PoisonerNameOverride, 13));
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        PoisonTarget = byte.MaxValue;
            
        PlayerControlEvents.OnPlayerMurder -= HandlePlayerMurder;
        PlayerControlEvents.OnPlayerDisconnect -= HandlePlayerDisconnect;
    }
        
    public override void OnFailedNonMeetingKill()
    {
        base.OnFailedNonMeetingKill();
        Button.CurrentTime = PlayerControl.GameOptions.KillCooldown;
    }
        
    public void HandlePlayerMurder(PlayerControl player, PlayerControl target)
    {
        if (Owner.AmOwner && player.AmOwner && target && target.Data.IsDead && !Owner.Data.IsDead) Button.CurrentTime = Button.Cooldown;
        if (target.PlayerId == PoisonTarget)
        {
            PoisonTarget = byte.MaxValue;

            Owner.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
            Button.CurrentTime = PlayerControl.GameOptions.KillCooldown;   
        }
    }

    public void HandlePlayerDisconnect(GameData.PlayerInfo player)
    {
        if (player.PlayerId == PoisonTarget)
        {
            PoisonTarget = byte.MaxValue;

            Owner.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
            Button.CurrentTime = PlayerControl.GameOptions.KillCooldown;   
        }
    }

    public string PoisonerNameOverride(PlayerControl player, string currentName, bool inMeeting)
    {
        if (!player.IsThere() || player.Data.IsDead) return currentName;
        if (player.PlayerId != PoisonTarget) return currentName;
        if (PlayerControl.LocalPlayer.GetRoleManager().MyRole.Alignment != Alignment.Impostor && !RoleManager.SeesRolesAsGhost()) return currentName;

        return $"{currentName}{(currentName.Contains("]") ? "" : " ")}<color=#{Settings.HeaderColor.ToRGBAString()}>[※]</color>";
    }

    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Poisoner_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public void Poison(PlayerControl target)
    {
        Owner.GetRoleManager().StartCoroutine(CoPoison(target));
        PoisonTarget = target.PlayerId;
    }

    public IEnumerator CoPoison(PlayerControl target)
    {
        for (float t = 0; t < KillDelay; t += Time.deltaTime)
        {
            yield return null;
                
            if (target.Data.IsDead || target.Data.Disconnected)
            {
                PoisonTarget = byte.MaxValue;
                    
                if (Owner.AmOwner)
                {
                    PlayerControl.LocalPlayer.GetRoleManager().StopCoroutine(Button.EffectCoroutine);
                        
                    Button.KillButtonManager.TimerText.color = UnityEngine.Color.white;
                    Button.CurrentTime = PlayerControl.GameOptions.KillCooldown;
                    PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
                }
                    
                yield break;
            }
        }

        PoisonTarget = byte.MaxValue;
            
        if (!MeetingHud.Instance)
        {
            List<PlayerControl> showFlashTo = new() {Owner};
            CrewmateRoles.MedicRole.Patches.PlayerControl_MurderPlayer_Patch.ShowFlashTo = showFlashTo;
            NeutralRoles.LawyerRole.Patches.PlayerControl_MurderPlayer_Patch.ShowFlashTo = showFlashTo;
                
            target.MurderPlayer(target);
            target.GetRoleManager().KilledBy = Owner.PlayerId;
        }
        else
        {
            GuessAbility guess = new(this);
            guess.AssassinKill(target.PlayerId);
        }
    }
}