using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.Localization.Languages;

namespace Lifeboat.GameOptions;

[OptionHeader(nameof(English.Lifeboat_GameOptions_General), 1000000)]
public static class GeneralOptions
{
    [NumberOption(nameof(English.Lifeboat_GameOptions_General_MaxCrewmate), "Max Crewmate Roles", 10,
        0, 15, 1, false, "{0}")]
    public static float CrewmateRoles = 0;

    [NumberOption(nameof(English.Lifeboat_GameOptions_General_MaxImpostor), "Max Impostor Roles", 9,
        0, 15, 1, false, "{0}")]
    public static float ImpostorRoles = 0;

    [NumberOption(nameof(English.Lifeboat_GameOptions_General_MaxNeutral), "Max Neutral Roles", 8,
        0, 15, 1, false, "{0}")]
    public static float NeutralRoles = 0;

    [ToggleOption(nameof(English.Lifeboat_GameOptions_General_EnableModifiers), "General_EnableModifiers", 7)]
    public static bool EnableModifiers = false;

    [ToggleOption(nameof(English.Lifeboat_GameOptions_General_GhostsSeeRoles), "Ghosts See Roles In Meetings", 6)]
    public static bool GhostsSeeRoles = true;

    [ToggleOption(nameof(English.Lifeboat_GameOptions_General_ImpostorsSeeTeammates), "Impostors See Impostor Roles", 5)]
    public static bool ImpsSeeRoles = true;

    [ToggleOption(nameof(English.Lifeboat_GameOptions_General_AllImpostorsCanAssassinate), "Lifeboat_AllImpsCanAssassinate", 4)]
    public static bool AllImpsCanAssassinate = true;
        
    [ToggleOption(nameof(English.Lifeboat_GameOptions_ShowMeaningless), "Lifeboat_ShowMeaningless", 3)]
    public static bool ShowMeaninglessOptions = false;
    public static bool ShowMeaninglessOptions_GetVisible => AmongUsClient.Instance.AmHost;

    public static bool ShouldShowMeaningless => ShowMeaninglessOptions && AmongUsClient.Instance.AmHost;
}