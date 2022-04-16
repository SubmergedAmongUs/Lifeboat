using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.IL2CPP;
using Framework;
using Framework.CustomOptions;
using Framework.Utilities;
using Lifeboat.Attributes;
using Lifeboat.Debugging;
using Lifeboat.Enums;
using Lifeboat.Events;
using Lifeboat.Extensions;
using Lifeboat.Roles;
using Lifeboat.StaticDataModifiers;
using Submerged;
using Submerged.Minigames.CustomMinigames.SpotWhaleShark;
using UnityEngine.SceneManagement;

namespace Lifeboat;

[BepInPlugin(nameof(LifeboatPlugin), "Lifeboat", "0.0.0")]
[BepInDependency(nameof(FrameworkPlugin))]
[BepInDependency(nameof(SubmergedPlugin))]
public sealed class LifeboatPlugin : SubPlugin<LifeboatPlugin>
{
    public override void Load()
    {
        if (!FrameworkPlugin.LifeboatEnabled)
        {
            ((BasePlugin) this).Log.LogMessage("Skipping loading Lifeboat as it is disabled.");
            return;
        }
            
        base.Load();
            
        LifeboatDebug.Instance = new LifeboatDebug();
        new PaletteModifier().Patch();
        CustomOptionsManager.Register(GetType().Assembly);
        CustomOptionsManager.EnableProfiles = true;
        CustomOptionsManager.StartOnModOptionsPage = true;
        WhaleSharkTask.CanComplete = player => player.GetRoleManager().MyRole.Alignment == Alignment.Crewmate;
        ResolveRoles();

        PluginEvents.OnSceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene)
    {
        PlayerControlEvents.Clear();
        MeetingHudEvents.Clear();
    }

    public void ResolveRoles()
    {
        foreach (Alignment faction in Enum.GetValues(typeof(Alignment)))
        {
            BaseRole.AlignmentToRoleTypes[faction] = new List<Type>();
        }
            
        IEnumerable<Type> roleTypes = GetType().Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(BaseRole)));
        foreach (Type type in roleTypes)
        {
            BaseRole testRole = (BaseRole) Activator.CreateInstance(type);
            BaseRole.AlignmentToRoleTypes[testRole.Alignment].Add(type);
            BaseRole.StringIDToRoleType[testRole.RoleStringID] = type;
            BaseRole.RoleTypeToStringID[type] = testRole.RoleStringID;
            BaseRole.RoleTypeToColor[type] = testRole.Color;
            BaseRole.RoleTypeToAmountField[type] = type.GetFields().FirstOrDefault(f => f.IsDefined(typeof(RoleAmountAttribute), false));
        }
    }
}