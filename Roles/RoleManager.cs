using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Attributes;
using Framework.Extensions;
using Lifeboat.Buttons;
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.GameOptions;
using Lifeboat.RoleModifiers;
using Lifeboat.Roles.CrewmateRoles;
using Lifeboat.Roles.CrewmateRoles.AltruistRole;
using Lifeboat.Roles.ImpostorRoles;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace Lifeboat.Roles;

[RegisterInIl2Cpp]
public sealed class RoleManager : MonoBehaviour
{
    public RoleManager(IntPtr ptr) : base(ptr) { }
        
    private BaseRole m_MyRole;
    private BaseModifier m_MyModifier;
        
    [HideFromIl2Cpp] public CustomImportantTextTask TextTask { get; set; }
    [HideFromIl2Cpp] public AppearanceManager AppearanceManager { get; set; }
    [HideFromIl2Cpp] public PlayerControl Owner { get; set; }
    [HideFromIl2Cpp] public BaseRole MyRole
    {
        get => m_MyRole;
        set
        {
            m_MyRole?.OnDestroy();
            m_MyRole = value;
            if (value == null) return;
            m_MyRole.Owner = Owner;
            m_MyRole.Awake();
        }
    }
    [HideFromIl2Cpp] public BaseModifier MyModifier
    {
        get => m_MyModifier;
        set
        {
            m_MyModifier?.OnDestroy();
            m_MyModifier = value;
            if (value == null) return;
            m_MyModifier.Owner = Owner;
            m_MyModifier.Awake();
        }
    }
    [HideFromIl2Cpp] public List<BaseButton> Buttons { get; } = new();
    [HideFromIl2Cpp] public byte KilledBy { get; set; } = byte.MaxValue;
    [HideFromIl2Cpp] public NameOverrideManager NameOverrides { get; } = new();
        
    public void Start()
    {
        Owner = GetComponent<PlayerControl>();
        MyRole = new Crewmate();
            
        if (Owner.notRealPlayer || Owner.isDummy) return;
        AppearanceManager = gameObject.EnsureComponent<AppearanceManager>();
            
        NameOverrides.Add(new NameOverride(DefaultNameOverride));
        NameOverrides.Add(new NameOverride(OtherImpostorsRoleNameOverride));
    }

    public void Update()
    {
        foreach (BaseButton b in Buttons.ToList())
        {
            b.Update();
        }

        if (Owner.AmOwner)
        {
            if (Input.GetKeyDown(KeyCode.F3)) PlayerControl.LocalPlayer.Collider.enabled = false;
            else if (Input.GetKeyUp(KeyCode.F3)) PlayerControl.LocalPlayer.Collider.enabled = true;
        }
    }

    public void LateUpdate()
    {
        MyRole?.Update();
        MyModifier?.Update();

        if (Owner.AmOwner && MyRole != null && ShipStatus.Instance && PlayerControl.LocalPlayer)
        {
            if (!TextTask)
            {
                TextTask = new GameObject("_Player - Custom Text").AddComponent<CustomImportantTextTask>();
                TextTask.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
                PlayerControl.LocalPlayer.myTasks.Insert(0, TextTask);
            }

            TextTask.Text = MyRole.GetImportantTaskText();
            if (MyModifier != null) TextTask.Text += $"\n{MyModifier.GetImportantTaskText()}";
        }
    }

    private void OnDestroy()
    {
        m_MyRole?.OnDestroy();
        m_MyModifier?.OnDestroy();
    }

    [HideFromIl2Cpp]
    public void SetRoles(List<(byte playerId, string stringID)> roles)
    {
        LifeboatPlugin.Log.LogInfo("Game started. Player: " + PlayerControl.LocalPlayer.name);
        List<BaseRole> assignedRoles = new();
        foreach ((byte playerId, string stringID) in roles)
        {
            BaseRole newRole = stringID switch
            {
                nameof(StringNames.Crewmate) => new Crewmate(),
                nameof(StringNames.Impostor) => new Impostor(),
                _ => (BaseRole) Activator.CreateInstance(BaseRole.StringIDToRoleType[stringID])
            };

            assignedRoles.Add(newRole);
            if (GameData.Instance.GetPlayerById(playerId)?.Object!?.GetRoleManager() is { } manager) manager.MyRole = newRole;
        }

        foreach (BaseRole baseRole in assignedRoles) baseRole.Start();
    }

    [HideFromIl2Cpp] public List<PlayerControl> GetIntroTeam() => MyRole.GetIntroTeam();
    [HideFromIl2Cpp] public void SetIntroAppearance(IntroCutscene introCutscene) => MyRole.SetIntroAppearance(introCutscene);

    public static bool CouldAppear(float amount, Alignment alignment)
    {
        if (GeneralOptions.ShouldShowMeaningless) return true;
            
        if (amount <= 0) return false;

        int availableImpostorSlots = Mathf.Min(Mathf.Min((int) GeneralOptions.ImpostorRoles, 
            PlayerControl.GameOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount)), PlayerControl.AllPlayerControls.Count);
        int maxedImpostorRoles = BaseRole.AlignmentToRoleTypes[Alignment.Impostor]
            .Select(t => BaseRole.RoleTypeToAmountField[t])
            .Where(f => f != null)
            .Select(f => (int) (float) f.GetValue(null))
            .Count(v => v >= 100);
            
        int availableNeutralSlots = Mathf.Min((int) GeneralOptions.NeutralRoles, 
            PlayerControl.AllPlayerControls.Count - Mathf.Min(availableImpostorSlots, maxedImpostorRoles));
        int maxedNeutralRoles = BaseRole.AlignmentToRoleTypes[Alignment.Neutral]
            .Select(t => BaseRole.RoleTypeToAmountField[t])
            .Where(f => f != null)
            .Select(f => (int) (float) f.GetValue(null))
            .Count(v => v >= 100);

        int availableCrewmateSlots = Mathf.Min((int) GeneralOptions.CrewmateRoles, 
            PlayerControl.AllPlayerControls.Count - Mathf.Min(availableImpostorSlots, maxedImpostorRoles) - Mathf.Min(availableNeutralSlots, maxedNeutralRoles));
        int maxedCrewmateRoles = BaseRole.AlignmentToRoleTypes[Alignment.Crewmate]
            .Select(t => BaseRole.RoleTypeToAmountField[t])
            .Where(f => f != null)
            .Select(f => (int) (float) f.GetValue(null))
            .Count(v => v >= 100);
            
        switch (alignment)
        {
            case Alignment.Impostor:
                return maxedImpostorRoles < availableImpostorSlots || (availableImpostorSlots > 0 && amount >= 100);
                
            case Alignment.Neutral:
                return maxedNeutralRoles < availableNeutralSlots || (availableNeutralSlots > 0 && amount >= 100);
                
            case Alignment.Crewmate:
                return maxedCrewmateRoles < availableCrewmateSlots || (availableCrewmateSlots > 0 && amount >= 100);
                
            default:
                return false;
        }
    }

    public static string DefaultNameOverride(PlayerControl player, string currentName, bool inMeeting)
    {
        BaseRole role = player!?.GetRoleManager()?.MyRole;
        return (player!.AmOwner || SeesRolesAsGhost()) && role != null ?
            $"<color=#{role.Color.ToRGBAString()}>{currentName}\n<size=70%><color=#{role.Color.ToRGBAString()}>{role.RoleName}" : currentName;
    }

    public static string OtherImpostorsRoleNameOverride(PlayerControl player, string currentName, bool inMeeting)
    {
        BaseRole role = player!?.GetRoleManager()?.MyRole;

        if (SeesRolesAsGhost()) return currentName;
            
        if (role != null && !player.AmOwner && role.Alignment == Alignment.Impostor && PlayerControl.LocalPlayer.GetRoleManager().MyRole.Alignment == Alignment.Impostor)
        {
            string colorString = GeneralOptions.ImpsSeeRoles ? role.Color.ToRGBAString() : Palette.ImpostorRed.ToRGBAString();
            string roleString = GeneralOptions.ImpsSeeRoles ? role.RoleName : TranslationController.Instance.GetString(StringNames.Impostor);
            return $"<color=#{colorString}>{currentName}\n<size=70%><color=#{colorString}>{roleString}";
        }
            
        return currentName;
    }

    public static bool CanBeRevived(PlayerControl player)
    {
        return FindObjectsOfType<DeadBody>().Any(b => b.ParentId == player.PlayerId) && !player.Data.Disconnected &&
               PlayerControl.AllPlayerControls.ToArray()
                   .Where(p => p.PlayerId != player.PlayerId)
                   .Any(p => p.GetRoleManager()!?.MyRole is Altruist && (!p.Data.IsDead || CanBeRevived(p)));
    }

    public static bool SeesRolesAsGhost()
    {
        return PlayerControl.LocalPlayer.Data.IsDead && GeneralOptions.GhostsSeeRoles && !CanBeRevived(PlayerControl.LocalPlayer);
    }
}