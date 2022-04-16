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
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.Roles.CrewmateRoles.SheriffRole;
using Lifeboat.Roles.NeutralRoles.GlitchRole;
using Lifeboat.Utils;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.SnitchRole;

[OptionHeader(nameof(English.Lifeboat_Snitch))]
public sealed class Snitch : Crewmate
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_CrewmateRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Snitch), "Snitch")] 
    public static float SnitchAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 93),
        HeaderColor = new Color32(255, 162, 112, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(SnitchAmount, Alignment.Crewmate),
    };

    [StringOption(nameof(English.Lifeboat_Snitch_GameOptions_CanSee), "Snitch_CanSee", 10,
        nameof(English.Lifeboat_GameOptions_Generic_Impostors), nameof(English.Lifeboat_GameOptions_Generic_ImpGlitch),
        nameof(English.Lifeboat_Snitch_GameOptions_CanSee_ImpGlitchSheriff))]
    public static int CanSee = 2;
        
    [ToggleOption(nameof(English.Lifeboat_Snitch_GameOptions_SeeKillersInMeeting), "Snitch Sees Impostor In Meeting", 9)] 
    public static bool SeeKillersInMeeting = false;
        
    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Snitch);
    public override Color32 Color => Settings.HeaderColor;

    public List<(PlayerControl player, ArrowBehaviour arrow)> Arrows { get; set; }
    public bool HasShownWarning { get; set; }

    public bool AmITargeted(BaseRole myRole = null)
    {
        myRole ??= PlayerControl.LocalPlayer.GetRoleManager().MyRole;
        return ((myRole.Alignment == Alignment.Impostor) || (myRole is Glitch && CanSee >= 1) || myRole is Sheriff && CanSee >= 2);
    }

    public override void Start()
    {
        if (Owner.AmOwner)
        {
            foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
            {
                RoleManager manager = playerControl!?.GetRoleManager();
                if (manager) manager.NameOverrides.Add(new NameOverride(SnitchSelfNameOverride, 3));
            }
        }
            
        Owner.GetRoleManager().NameOverrides.Add(new NameOverride(SnitchOthersNameOverride, 2));
    }

    public override void Update()
    {
        if (Owner.AmOwner) UpdateSnitch();
        else if (AmITargeted()) UpdateTarget();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (Arrows != null)
        {
            foreach ((_, ArrowBehaviour arrow) in Arrows)
            {
                if (arrow.IsThere()) arrow.gameObject.SetActive(false);
            }
        }
    }

    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Snitch_IntroText, Color.ToRGBAString(), 
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public string SnitchSelfNameOverride(PlayerControl player, string currentName, bool inMeeting)
    {
        if (!player.IsThere()) return currentName;
        if (!inMeeting || !SeeKillersInMeeting || Arrows == null) return currentName;
        if (RoleManager.SeesRolesAsGhost()) return currentName;
            
        BaseRole theirRole = player!.GetRoleManager()?.MyRole;
        if (theirRole != null && Arrows.Any(a => a.player.PlayerId == player.PlayerId) && CanSee switch
            {
                >= 0 when theirRole.Alignment == Alignment.Impostor => true,
                >= 1 when theirRole is Glitch => true,
                >= 2 when theirRole is Sheriff => true,
                _ => false,
            })
        {
            return $"<color=#{Color.ToRGBAString()}>{currentName}\n<size=70%><color=#{Color.ToRGBAString()}>{LanguageProvider.Current.Lifeboat_Snitch_UI_Killer}";
        }

        return currentName;
    }
        
    public string SnitchOthersNameOverride(PlayerControl player, string currentName, bool inMeeting)
    {
        if (!player.IsThere()) return currentName;
        if (!AmITargeted() || Arrows == null) return currentName;
        if (RoleManager.SeesRolesAsGhost()) return currentName;

        if (Arrows.Any(a => a.player.PlayerId == player.PlayerId))
        {
            return $"<color=#{Color.ToRGBAString()}>{currentName}\n<size=70%><color=#{Color.ToRGBAString()}>{RoleName}";
        }

        return currentName;
    }
        
    public void UpdateSnitch()
    {
        if (Arrows == null && Owner.Data?.Tasks?.ToArray().All(t => t.Complete) == true)
        {
            Arrows = new List<(PlayerControl player, ArrowBehaviour arrow)>();
            foreach (PlayerControl player in GameData.Instance.AllPlayers.ToArray()
                         .Where(p => AmITargeted(p.Object!?.GetRoleManager()?.MyRole)).Select(p => p.Object))
            {
                GameObject arrow = MapLoader.Skeld.CommonTasks
                    .First(t => t!?.GetComponentInChildren<ArrowBehaviour>(true))
                    .GetComponentInChildren<ArrowBehaviour>(true).gameObject;
                ArrowBehaviour arrowBehaviour = GameObject.Instantiate(arrow).GetComponent<ArrowBehaviour>();
                arrowBehaviour.gameObject.SetActive(true);
                arrowBehaviour.transform.parent = Owner.transform;
                arrowBehaviour.image = arrowBehaviour.GetComponent<SpriteRenderer>();
                arrowBehaviour.image.color = Color;
                    
                Arrows.Add((player, arrowBehaviour));
            }
        }

        if (Arrows != null)
        {
            foreach ((PlayerControl player, ArrowBehaviour arrow) in Arrows)
            {
                if (PlayerControl.LocalPlayer.Data.IsDead || !player.IsThere() || player.Data == null || player.Data.IsDead) arrow.gameObject.SetActive(false);
                else arrow.target = player.transform.position;
            }
        }
    }

    public void UpdateTarget()
    {
        if (!ShipStatus.Instance || Owner.Data?.Tasks == null || Owner.Data.Tasks.Count == 0 || Owner.Data.IsDead)
        {
            if (Arrows != null) foreach ((_, ArrowBehaviour arrow) in Arrows)
            {
                arrow.gameObject.SetActive(false);
            }
            return;
        }

        if (!HasShownWarning && Owner.Data.Tasks.ToArray().Count(t => !t.Complete) <= 0)
        {
            HasShownWarning = true;
            Owner.GetRoleManager().StartCoroutine(UsefulMethods.CoFlashScreen(new Color32(255, 207, 112, 77)));
            UsefulMethods.ShowTextToast(string.Format(LanguageProvider.Current.Lifeboat_Snitch_Toast_Finished, Color.ToRGBAString()), 1.5f);
        }
        else if (Arrows == null && Owner.Data.Tasks.ToArray().Count(t => !t.Complete) <= 1)
        {
            Owner.GetRoleManager().StartCoroutine(UsefulMethods.CoFlashScreen(new Color32(255, 207, 112, 77)));
            Arrows = new List<(PlayerControl player, ArrowBehaviour arrow)>();
                
            GameObject arrow = MapLoader.Skeld.CommonTasks
                .First(t => t!?.GetComponentInChildren<ArrowBehaviour>(true))
                .GetComponentInChildren<ArrowBehaviour>(true).gameObject;
            ArrowBehaviour arrowBehaviour = GameObject.Instantiate(arrow).GetComponent<ArrowBehaviour>();
            arrowBehaviour.gameObject.SetActive(true);
            arrowBehaviour.transform.parent = PlayerControl.LocalPlayer.transform;
            arrowBehaviour.image = arrowBehaviour.GetComponent<SpriteRenderer>();
            arrowBehaviour.image.color = Color;
                    
            Arrows.Add((Owner, arrowBehaviour));
            if (!HasShownWarning)
            {
                UsefulMethods.ShowTextToast(string.Format(LanguageProvider.Current.Lifeboat_Snitch_Toast_AlmostFinished, Color.ToRGBAString()), 1.5f);
            }
        }

        if (Arrows != null) foreach ((PlayerControl player, ArrowBehaviour arrow) in Arrows)
        {
            arrow.target = player.transform.position;
            if (PlayerControl.LocalPlayer.Data.IsDead || !player || player.Data == null || player.Data.IsDead) arrow.gameObject.SetActive(false);
        }
    }
}