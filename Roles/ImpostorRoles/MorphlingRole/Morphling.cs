using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Lifeboat.Attributes;
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.Roles.ImpostorRoles.MorphlingRole.Buttons;

namespace Lifeboat.Roles.ImpostorRoles.MorphlingRole;

[OptionHeader(nameof(English.Lifeboat_Morphling))]
public sealed class Morphling : Impostor
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_ImpostorRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Morphling), "Morphling")] 
    public static float MorphlingAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 54),
        HeaderColor = Palette.ImpostorRed,
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(MorphlingAmount, Alignment.Impostor),
    };

    [NumberOption(nameof(English.Lifeboat_Morphling_GameOptions_Duration), "Morph Duration", 10,
        3, 25, 1f, false, "{0}s")] 
    public static float MorphlingDuration = 10;
        
    [NumberOption(nameof(English.Lifeboat_Morphling_GameOptions_Cooldown), "Morphling Cooldown", 9,
        5, 60, 2.5f, false, "{0:0.0}s")] 
    public static float MorphlingCooldown = 25;

    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Morphling);

    public PlayerControl Sampled { get; set; }

    public override void Start()
    {
        base.Start();
        if (Owner.AmOwner)
        {
            new SampleButton(this);
            new MorphButton(this);
        }
    }

    public override bool CanUseVents() => false;

    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Morphling_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public void Morph(byte id, float duration)
    {
        GameData.PlayerInfo sampledData = GameData.Instance.GetPlayerById(id);
        Sampled = sampledData.Object;

        AppearanceModification modification = new()
        {
            Data = new AppearanceData
            {
                Name = Sampled.nameText.text,
                ColorId = sampledData.ColorId,
                HatId = sampledData.HatId,
                SkinId = sampledData.SkinId,
                PetId = sampledData.PetId,
            },
            ModificationMask = AppearanceModification.Overrides.Name |
                               AppearanceModification.Overrides.ColorId |
                               AppearanceModification.Overrides.HatId |
                               AppearanceModification.Overrides.SkinId |
                               AppearanceModification.Overrides.PetId,
            Priority = 1,
            Timer = duration
        };

        Owner.GetRoleManager().AppearanceManager.Modifications.Add(modification);
    }
}