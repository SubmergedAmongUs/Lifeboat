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
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.RoleAbilities.SwapAbility;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.SwapperRole;

[OptionHeader(nameof(English.Lifeboat_Swapper))]
public sealed class Swapper : Crewmate
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_CrewmateRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Swapper), "Swapper")] 
    public static float SwapperAmount = 0;
        
    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 92),
        HeaderColor = new Color32(255, 112, 165, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(SwapperAmount, Alignment.Crewmate),
    };

    [ToggleOption("Submerged", "Swapper_Unused")]
    public static bool UnusedOption = false;
    public static bool UnusedOption_GetVisible() => false;

    #endregion

    public override string RoleStringID => nameof(English.Lifeboat_Swapper);
    public override Color32 Color => Settings.HeaderColor;

    public override void Start()
    {
        Abilities.Add(new SwapAbility(this, 5));
    }

    public override bool IsJousting() => PlayerControl.AllPlayerControls.ToArray()
        .Count(p => p.IsThere() && p.Data is {IsDead: false, Disconnected: false, IsImpostor: true}) <= 1;
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Swapper_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }
}