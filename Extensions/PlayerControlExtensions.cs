using System.Collections.Generic;
using Framework.Extensions;
using Framework.Utilities;
using Lifeboat.CustomAppearance;
using Lifeboat.Roles;

namespace Lifeboat.Extensions;

public static class PlayerControlExtensions
{
    public static Dictionary<PlayerControl, RoleManager> RoleManagers = new(Il2CppEqualityComparer<PlayerControl>.Instance);
    public static RoleManager GetRoleManager(this PlayerControl player)
    {
        if (!player) return null;
        if (RoleManagers.TryGetValue(player, out RoleManager manager)) return manager;
        RoleManager roleManager = player.GetComponent<RoleManager>();
        if (roleManager == null) return null;
        return RoleManagers[player] = roleManager;
    }
        
    public static Dictionary<PlayerControl, AppearanceManager> AppearanceManagers = new(Il2CppEqualityComparer<PlayerControl>.Instance);
    public static AppearanceManager GetAppearanceManager(this PlayerControl player)
    {
        if (!player) return null;
        if (AppearanceManagers.TryGetValue(player, out AppearanceManager manager)) return manager;
        AppearanceManager roleManager = player.gameObject.EnsureComponent<AppearanceManager>();
        if (roleManager == null) return null;
        return AppearanceManagers[player] = roleManager;
    }
}