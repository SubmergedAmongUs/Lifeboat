using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Lifeboat.Attributes;
using Lifeboat.Enums;
using Lifeboat.Events;
using Lifeboat.Extensions;
using Lifeboat.GameOptions;
using Lifeboat.Roles.CrewmateRoles.EngineerRole.Buttons;
using Submerged;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.EngineerRole;

[OptionHeader(nameof(English.Lifeboat_Engineer))]
public sealed class Engineer : Crewmate
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_CrewmateRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Engineer), "Engineer")] public static float EngineerAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 99),
        HeaderColor = new Color32(248, 191, 20, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(EngineerAmount, Alignment.Crewmate),
    };

    [NumberOption(nameof(English.Lifeboat_Engineer_GameOptions_FixAmount), "Engineer_FixAmount", 10,
        1, 3, 1, false, "{0}")]
    public static float FixAmount = 2;

    [ToggleOption(nameof(English.Lifeboat_Engineer_GameOptions_FixMultipleTimesPerRound), "Engineer_FixMultipleTimesPerRound", 9)]
    public static bool CanFixMultipleTimesPerRound = true;
    public static bool CanFixMultipleTimesPerRound_GetVisible => FixAmount > 1 || GeneralOptions.ShouldShowMeaningless;

    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Engineer);
    public override Color32 Color => Settings.HeaderColor;

    public FixButton Button;
        
    public override void Start()
    {
        if (Owner.AmOwner)
        {
            Button = new FixButton(this);
            MeetingHudEvents.OnMeetingStart += HandleMeetingHudStart;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        MeetingHudEvents.OnMeetingStart -= HandleMeetingHudStart;
    }

    public void HandleMeetingHudStart(MeetingHud meetingHud)
    {
        if (CanFixMultipleTimesPerRound == false && Button != null) Button.IsUseAvailable = true;
    }
        
    public override bool CanUseVents() => true;

    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(ShipStatus.Instance.Type switch
        {
            ShipStatus.MapType.Ship => LanguageProvider.Current.Lifeboat_Engineer_IntroText_Skeld,
            ShipStatus.MapType.Hq => LanguageProvider.Current.Lifeboat_Engineer_IntroText_Mira,
            ShipStatus.MapType.Pb => LanguageProvider.Current.Lifeboat_Engineer_IntroText_Polus,
            (ShipStatus.MapType) 3 => LanguageProvider.Current.Lifeboat_Engineer_IntroText_Airship,
            SubmergedPlugin.MAP_TYPE => LanguageProvider.Current.Lifeboat_Engineer_IntroText_Submerged,
            _ => "",
        }, Color.ToRGBAString(), Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }
}