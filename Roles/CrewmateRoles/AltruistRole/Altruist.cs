using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Framework.Utilities;
using Lifeboat.Attributes;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.GameOptions;
using Lifeboat.Roles.CrewmateRoles.AltruistRole.Buttons;
using Lifeboat.Roles.CrewmateRoles.SheriffRole;
using Lifeboat.Roles.ImpostorRoles.BomberRole;
using Lifeboat.Roles.ImpostorRoles.UndertakerRole;
using Lifeboat.Roles.ImpostorRoles.UndertakerRole.Buttons;
using Lifeboat.Roles.NeutralRoles.GlitchRole;
using Lifeboat.Utils;
using Submerged.Map.MonoBehaviours;
using Submerged.Systems.CustomSystems.PlayerFloor.MonoBehaviours;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.AltruistRole;

[OptionHeader(nameof(English.Lifeboat_Altruist))]
public sealed class Altruist : Crewmate
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_CrewmateRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Altruist), "Altruist")] 
    public static float AltruistAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 100),
        HeaderColor = new Color32(68, 224, 196, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(AltruistAmount, Alignment.Crewmate),
    };

    [NumberOption(nameof(English.Lifeboat_Altruist_GameOptions_Duration), "Revive Duration", 10,
        3, 15, 1, false, "{0}s")] 
    public static float ReviveDuration = 10;

    [StringOption(nameof(English.Lifeboat_Altruist_GameOptions_ShowArrowTo), "Altruist_ShowArrowTo", 9,
        nameof(English.Lifeboat_Altruist_GameOptions_ShowArrowTo_NoOne), nameof(English.Lifeboat_GameOptions_Generic_Impostors), 
        nameof(English.Lifeboat_GameOptions_Generic_ImpGlitch))]
    public static int ShowArrowTo = 0;

    [ToggleOption(nameof(English.Lifeboat_Altruist_GameOptions_ShowArrowToKiller), "Altruist_ShowArrowToKiller", 8)]
    public static bool ShowArrowToKiller = true;

    [StringOption(nameof(English.Lifeboat_Altruist_GameOptions_ShowHeadStart), "Altruist_ShowHeadStart", 7,
        nameof(StringNames.SettingsOff), nameof(English.Lifeboat_Altruist_GameOptions_ShowHeadStart_OnStart),
        nameof(English.Lifeboat_Altruist_GameOptions_ShowHeadStart_OnHalfwayDone))]
    public static int ShowHeadStart = 2;
    public static bool ShowHeadStart_GetVisible => ShowArrowTo > 0 || ShowArrowToKiller || GeneralOptions.ShouldShowMeaningless;
        
    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Altruist);
    public override Color32 Color => Settings.HeaderColor;

    public override bool IsJousting() => IsReviving != byte.MaxValue || base.IsJousting();

    public List<(ArrowBehaviour arrow, PlayerControl player)> Arrows { get; set; } = new();
    public byte IsReviving { get; set; } = byte.MaxValue;

    public List<Undertaker> Undertakers = new();
        
    public override void Start()
    {
        if (Owner.AmOwner)
        {
            new ReviveButton(this);
        }

        if (PlayerControl.LocalPlayer.GetRoleManager().MyRole is Undertaker {DragDropButton: { } dragButton})
        {
            if (!dragButton.Altruists.Contains(this)) dragButton.Altruists.Add(this);
        }
            
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            if (playerControl.IsThere() && playerControl.Data is {Disconnected: false} && playerControl.GetRoleManager()!?.MyRole is Undertaker undertaker)
            {
                if (!Undertakers.Contains(undertaker)) Undertakers.Add(undertaker);
            }
        }
    }

    public override void Update()
    {
        foreach ((ArrowBehaviour arrow, PlayerControl player) in Arrows)
        {
            if (arrow != null && !player.WasCollected && player)
            {
                if (PlayerControl.LocalPlayer.Data.IsDead || player.Data is not {IsDead: false})
                {
                    arrow.gameObject.SetActive(false);
                    return;
                }

                arrow.target = player.transform.position;
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        foreach ((ArrowBehaviour arrow, _) in Arrows)
        {
            if (arrow.IsThere()) arrow.gameObject.SetActive(false);
        }
    }
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Altruist_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }
        
    public void RevivePlayer(byte playerId, Vector2 position)
    {
        GameData.PlayerInfo playerData = GameData.Instance.GetPlayerById(playerId);
        if (!playerData.Disconnected) Owner.GetRoleManager().StartCoroutine(CoRevive(playerData.Object, position));
    }

    public IEnumerator CoRevive(PlayerControl toBeRevived, Vector2 position)
    {
        IsReviving = toBeRevived.PlayerId;

        foreach (Undertaker undertaker in Undertakers)
        {
            if (undertaker.Corpse.IsThere() && undertaker.Corpse.Dragging && undertaker.Corpse.Parent.IsThere() && undertaker.Corpse.Parent.ParentId == IsReviving)
            {
                undertaker.Drop(undertaker.Corpse.Parent);
            }
        }
            
        SpriteRenderer overlay = GameObject.Instantiate(HudManager.Instance.FullScreen.gameObject, HudManager.Instance.transform).GetComponent<SpriteRenderer>();
        overlay.transform.SetZPos(overlay.transform.position.z - 1);
        overlay.gameObject.SetActive(true);
        overlay.enabled = true;
        overlay.color = UnityEngine.Color.clear;

        Owner.MurderPlayer(Owner);

        if (ShowHeadStart == 1)
        {
            if ((ShowArrowTo >= 1 && PlayerControl.LocalPlayer.GetRoleManager().MyRole.Alignment == Alignment.Impostor) ||
                (ShowArrowTo == 2 && PlayerControl.LocalPlayer.GetRoleManager().MyRole is Glitch) || 
                (ShowArrowToKiller && toBeRevived.GetRoleManager().KilledBy == PlayerControl.LocalPlayer.PlayerId))
            {
                UsefulMethods.ShowTextToast(string.Format(LanguageProvider.Current.Lifeboat_Altruist_Toast_ReviveInProgress, Color.ToRGBAString(),
                    toBeRevived.Data.PlayerName), 1.5f);
                    
                PlayerControl.LocalPlayer.GetRoleManager().StartCoroutine(UsefulMethods.CoFlashScreen(new Color32(68, 224, 196, 77)));
            }
        }

        bool hasShownHeadStart = ShowHeadStart != 2;
        for (float t = 0; t < ReviveDuration; t += Time.deltaTime)
        {
            if (!Owner.IsThere() || !toBeRevived.IsThere() || MeetingHud.Instance)
            {
                overlay.gameObject.Destroy();
                IsReviving = byte.MaxValue;
                yield break;
            }
                
            if (!hasShownHeadStart && t * 2 >= ReviveDuration)
            {
                hasShownHeadStart = true;
                    
                if ((ShowArrowTo >= 1 && PlayerControl.LocalPlayer.GetRoleManager().MyRole.Alignment == Alignment.Impostor) ||
                    (ShowArrowTo == 2 && PlayerControl.LocalPlayer.GetRoleManager().MyRole is Glitch) || 
                    (ShowArrowToKiller && toBeRevived.GetRoleManager().KilledBy == PlayerControl.LocalPlayer.PlayerId))
                {
                    UsefulMethods.ShowTextToast(string.Format(LanguageProvider.Current.Lifeboat_Altruist_Toast_ReviveInProgress, Color.ToRGBAString(),
                        toBeRevived.Data.PlayerName), 1.5f);
                        
                    PlayerControl.LocalPlayer.GetRoleManager().StartCoroutine(UsefulMethods.CoFlashScreen(new Color32(68, 224, 196, 77)));
                }
            }
                
            if (!Owner.AmOwner && !toBeRevived.AmOwner)
            {
                yield return null;
                continue;
            }

            overlay.enabled = true;
            overlay.color = Color32.Lerp(UnityEngine.Color.clear, new Color32(68, 224, 196, 100), t / ReviveDuration);
            yield return null;
        }
            
        overlay.gameObject.Destroy();

        if (!Owner.IsThere() || !toBeRevived.IsThere())
        {
            IsReviving = byte.MaxValue;
            yield break;
        }

        toBeRevived.Revive();
        toBeRevived.MyPhysics.ResetMoveState();
        if (toBeRevived.AmOwner)
        {
            if (SubmarineStatus.Instance) FloorHandler.GetFloorHandler(PlayerControl.LocalPlayer).RpcRequestChangeFloor(position.y > -6f);
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
        }
            
        DeadBodyExtensions.DestroyBody(Owner.PlayerId);
        DeadBodyExtensions.DestroyBody(toBeRevived.PlayerId);

        if (PlayerControl.LocalPlayer.GetRoleManager().MyRole is Sheriff sheriff)
        {
            sheriff.Killed.RemoveAll(b => b == toBeRevived.PlayerId);
        }

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.IsThere() && player.GetRoleManager().IsThere() && player.GetRoleManager().MyRole is Bomber bomber)
            {
                bomber.CannotReport.RemoveAll(b => b == toBeRevived.PlayerId);
            }
        }

        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            IsReviving = byte.MaxValue;
            yield break;
        }

        if ((ShowArrowTo >= 1 && PlayerControl.LocalPlayer.GetRoleManager().MyRole.Alignment == Alignment.Impostor) ||
            (ShowArrowTo == 2 && PlayerControl.LocalPlayer.GetRoleManager().MyRole is Glitch) || 
            (ShowArrowToKiller && toBeRevived.GetRoleManager().KilledBy == PlayerControl.LocalPlayer.PlayerId))
        {
            GameObject arrow = MapLoader.Skeld.CommonTasks
                .First(t => t!?.GetComponentInChildren<ArrowBehaviour>(true))
                .GetComponentInChildren<ArrowBehaviour>(true).gameObject;
            ArrowBehaviour arrowBehaviour = GameObject.Instantiate(arrow).GetComponent<ArrowBehaviour>();
            arrowBehaviour.gameObject.SetActive(true);
            arrowBehaviour.transform.parent = PlayerControl.LocalPlayer.transform;
            arrowBehaviour.image = arrowBehaviour.GetComponent<SpriteRenderer>();
            arrowBehaviour.image.color = Color;

            Arrows.Add((arrowBehaviour, toBeRevived));
            toBeRevived.GetRoleManager().KilledBy = byte.MaxValue;
                
            UsefulMethods.ShowTextToast(string.Format(LanguageProvider.Current.Lifeboat_Altruist_Toast_Revived, Color.ToRGBAString(),
                toBeRevived.Data.PlayerName), 1.5f);
                
            PlayerControl.LocalPlayer.GetRoleManager().StartCoroutine(UsefulMethods.CoFlashScreen(new Color32(68, 224, 196, 77)));
        }
            
        IsReviving = byte.MaxValue;
    }
}