using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Lifeboat.Attributes;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.GameOptions;
using Lifeboat.RoleAbilities.GuessAbility;
using Lifeboat.RoleModifiers.Modifiers.LoversModifier;

namespace Lifeboat.Roles.ImpostorRoles.AssassinRole;

[OptionHeader(nameof(English.Lifeboat_Assassin))]
public sealed class Assassin : Impostor
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_ImpostorRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Assassin), "Assassin")] 
    public static float AssassinAmount = 0;
    public static bool AssassinAmount_GetVisible => !GeneralOptions.AllImpsCanAssassinate;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 60),
        HeaderColor = Palette.ImpostorRed,
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(AssassinAmount, Alignment.Impostor) || 
                             (GeneralOptions.AllImpsCanAssassinate && PlayerControl.GameOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount) > 0),
    };

    [NumberOption(nameof(English.Lifeboat_Assassin_GameOptions_MaxKillsPerMeeting), "Assassin_MaxKillsPerMeeting", 10,
        0, 5, 1, true, "{0}")]
    public static float MaxKillsPerMeeting = 0;

    [ToggleOption(nameof(English.Lifeboat_Assassin_GameOptions_CanGuessCrewmate), "Assassin_CanGuessCrewmate", 9)]
    public static bool CanGuessCrewmate = false;
        
    [ToggleOption(nameof(English.Lifeboat_Assassin_GameOptions_CanGuessLovers), "Assassin_CanGuessLovers", 8)]
    public static bool CanGuessLovers = true;
    public static bool CanGuessLovers_GetVisible => (GeneralOptions.EnableModifiers && Lovers.LoversAmount > 0 && GameData.Instance.PlayerCount > 1) || 
                                                    GeneralOptions.ShouldShowMeaningless;

    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Assassin);

    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Assassin_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public override void Start()
    {
        GuessAbility = new GuessAbility(this);
        GuessAbility.Start();
            
        Abilities.Add(GuessAbility);
    }
}