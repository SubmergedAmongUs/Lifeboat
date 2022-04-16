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
using Lifeboat.GameOptions;
using Lifeboat.Roles.CrewmateRoles.MedicRole.Buttons;
using Lifeboat.Utils;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.MedicRole;

[OptionHeader(nameof(English.Lifeboat_Medic))]
public sealed class Medic : Crewmate
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_CrewmateRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Medic), "Medic")] 
    public static float MedicAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 96),
        HeaderColor = new Color32(70, 235, 52, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(MedicAmount, Alignment.Crewmate),
    };

    [StringOption(nameof(English.Lifeboat_Medic_GameOptions_CanShieldEach), "Medic_ShieldPer", 10, 
        nameof(English.Lifeboat_Medic_GameOptions_CanShieldEach_Cooldown), nameof(English.Lifeboat_GameOptions_Generic_Round), 
        nameof(English.Lifeboat_GameOptions_Generic_Game))]
    public static int ShieldPer = 2;
        
    [ToggleOption(nameof(English.Lifeboat_Medic_GameOptions_CanSwapShield), "Medic_CanSwapShield", 9)]
    public static bool CanSwapShield = false;
    public static bool CanSwapShield_GetVisible => ShieldPer <= 1 || GeneralOptions.ShouldShowMeaningless;
        
    [NumberOption(nameof(English.Lifeboat_Medic_GameOptions_Cooldown), "Shield Cooldown", 8,
        5, 60, 2.5f, false, "{0:0.0}s")] 
    public static float ShieldCooldown = 10;

    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Medic);
    public override Color32 Color => Settings.HeaderColor;

    public byte Protected { get; set; } = byte.MaxValue;

    public override void Start()
    {
        if (Owner.AmOwner)
        {
            new MonitorButton(this);
            MeetingHudEvents.OnMeetingStart += HandleMeetingHudStart;
        }
            
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            RoleManager manager = playerControl!?.GetRoleManager();
            if (manager) manager.NameOverrides.Add(new NameOverride(MedicNameOverride, 10));
        }
    }
        
    public override void Update()
    {
        GameData.PlayerInfo player = GameData.Instance.GetPlayerById(Protected);
        if (Owner.Data.IsDead || Owner.Data.Disconnected || player is {IsDead: true} or {Disconnected: true}) Protected = byte.MaxValue;
    }
        
    public override void OnDestroy()
    {
        base.OnDestroy();
        MeetingHudEvents.OnMeetingStart -= HandleMeetingHudStart;
    }
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Medic_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }
        
    public void HandleMeetingHudStart(MeetingHud meetingHud)
    {
        if (ShieldPer == 1) new MonitorButton(this);
    }

    public void RpcProtect(PlayerControl player)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Medic_Monitor, SendOption.Reliable);
        writer.Write(player.PlayerId);
        writer.EndMessage();
            
        Protect(player);
    }

    public void Protect(PlayerControl player)
    {
        Protected = player.PlayerId;
    }

    public string MedicNameOverride(PlayerControl player, string currentName, bool _)
    {
        if (!player.IsThere() || player.Data.IsDead) return currentName;
        if (player.PlayerId != Protected) return currentName;
        if (PlayerControl.LocalPlayer.PlayerId != Owner.PlayerId && !RoleManager.SeesRolesAsGhost()) return currentName;

        return $"{currentName}{(currentName.Contains("]") ? "" : " ")}<color=#{Color.ToRGBAString()}>[♥]</color>";
    }

    public static bool CheckMedicProtection(PlayerControl killer, PlayerControl target, bool inMeeting, List<PlayerControl> showFlashTo = null)
    {
        showFlashTo ??= new List<PlayerControl> {killer};
            
        foreach (PlayerControl medicPlayer in PlayerControl.AllPlayerControls)
        {
            if (medicPlayer.Data.IsDead) continue;
                
            if (medicPlayer.GetRoleManager().MyRole is not Medic medic) continue;
            if (medic.Protected != target.PlayerId) continue;

            if (!inMeeting && killer.AmOwner) killer.GetRoleManager().MyRole.OnFailedNonMeetingKill();

            if (medicPlayer.AmOwner || showFlashTo.Select(p => p.PlayerId).Contains(PlayerControl.LocalPlayer.PlayerId))
            {
                medicPlayer.GetRoleManager().StartCoroutine(UsefulMethods.CoFlashScreen(new Color32(70, 235, 52, 77)));
            }
                    
            medic.Protected = byte.MaxValue;
            return true;
        }
            
        return false;
    }
}