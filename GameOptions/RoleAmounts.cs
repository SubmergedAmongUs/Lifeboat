using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Localization.Languages;
using UnityEngine;

namespace Lifeboat.GameOptions;

public static class CrewmateRoleAmounts
{
    [OptionHeader(nameof(English.Lifeboat_GameOptions_CrewmateRoles), 99999)]
    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 99999),
        HeaderColor = new Color32(140, byte.MaxValue, byte.MaxValue, byte.MaxValue),
        DefaultOpenInConsole = false,
        GroupVisible = () => GeneralOptions.CrewmateRoles > 0 || GeneralOptions.ShouldShowMeaningless,
    };
}

public static class ImpostorRoleAmounts
{
    [OptionHeader(nameof(English.Lifeboat_GameOptions_ImpostorRoles), 99998)]
    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 99998),
        HeaderColor = Palette.ImpostorRed,
        DefaultOpenInConsole = false,
        GroupVisible = () => Mathf.Min(PlayerControl.GameOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount), GeneralOptions.ImpostorRoles) > 0
                             || GeneralOptions.ShouldShowMeaningless,
    };
}

public static class NeutralRoleAmounts
{    
    [OptionHeader(nameof(English.Lifeboat_GameOptions_NeutralRoles), 99997)]
    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 99997),
        HeaderColor = new Color32(227, 128, 255, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => GeneralOptions.NeutralRoles > 0 || GeneralOptions.ShouldShowMeaningless,
    };
}

public static class RoleModifiersAmounts
{
    [OptionHeader(nameof(English.Lifeboat_GameOptions_RoleModifiers), 99996)]
    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 99996),
        HeaderColor = new Color32(255, 255, 0, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => GeneralOptions.EnableModifiers || GeneralOptions.ShouldShowMeaningless,
    };
}