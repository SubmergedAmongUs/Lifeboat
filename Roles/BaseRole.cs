using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.Extensions;
using Framework.Localization;
using Hazel;
using Lifeboat.Buttons;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.RoleAbilities;
using Lifeboat.Roles.ImpostorRoles.BomberRole.Buttons;
using UnityEngine;

namespace Lifeboat.Roles;

public abstract class BaseRole
{
    public static readonly Dictionary<Alignment, List<Type>> AlignmentToRoleTypes = new();
    public static readonly Dictionary<string, Type> StringIDToRoleType = new();
    public static readonly Dictionary<Type, string> RoleTypeToStringID = new();
    public static readonly Dictionary<Type, Color32> RoleTypeToColor = new();
    public static readonly Dictionary<Type, FieldInfo> RoleTypeToAmountField = new();

    public abstract string RoleStringID { get; }
    public virtual string RoleName => LanguageProvider.GetLocalizedString(RoleStringID);
    public abstract Color32 Color { get; }
    public abstract Alignment Alignment { get; }

    public PlayerControl Owner { get; set; }
    public List<BaseAbility> Abilities { get; set; } = new();
    public bool IsDestroyed { get; private set; }

    public virtual void Awake() { }
    public virtual void Start() { }
    public virtual void Update() { }

    public virtual void OnDestroy()
    {
        IsDestroyed = true;
            
        if (Owner && Owner.AmOwner)
        {
            RoleManager roleManager = Owner.GetRoleManager();
            if (!roleManager) return;
            foreach (BaseButton button in roleManager.Buttons.ToArray())
            {
                if (!button.IsModifierButton)
                {
                    button.GameObject.Destroy();
                    roleManager.Buttons.Remove(button);
                }
            }
        }
    }

    public string PreviousRole { get; set; } = "";

    public virtual List<PlayerControl> GetIntroTeam()
    {
        switch (Alignment)
        {
            case Alignment.Crewmate: return PlayerControl.AllPlayerControls.ToSystemList();
            case Alignment.Impostor: return PlayerControl.AllPlayerControls.ToArray().Where(p => p.Data.IsImpostor).ToList();
            default: return PlayerControl.LocalPlayer.ItemToList();
        }
    }
    public virtual void SetIntroAppearance(IntroCutscene introCutscene) { }
    public virtual string GetGameSummaryDescription(bool hasWon)
    {
        return $"{(hasWon ? "<color=green>" : "")}{Owner.Data.PlayerName}:{(hasWon ? "</color>" : "")} " +
               $"{PreviousRole}" +
               $"<color=#{Color.ToRGBAString()}>{RoleName}</color>" +
               $"{(Owner.GetRoleManager().MyModifier is { } modifier ? " " + modifier.GetGameSummaryDescription() : "")}";
    }

    public virtual string GetImportantTaskText() => string.Format(LanguageProvider.Current.Lifeboat_UI_TaskText, Color.ToRGBAString(), RoleName);
        
    public virtual bool CanUseCrewmateConsoles() => Alignment == Alignment.Crewmate;
    public virtual bool CanUseVents() => Alignment == Alignment.Impostor;

    public virtual bool IsJousting() => Owner!?.GetRoleManager()!?.Buttons.Any(b => b is ThrowBombButton) ?? false;

    public virtual void OnFailedNonMeetingKill() { }
        
    public virtual void Deserialize(MessageReader reader) { }
    public virtual void Serialize(MessageWriter writer) { }
    public virtual BaseRole CreateClone() => (BaseRole) Activator.CreateInstance(GetType());
}