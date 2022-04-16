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
using Lifeboat.Roles.ImpostorRoles.CamouflagerRole.Buttons;
using Object = UnityEngine.Object;

namespace Lifeboat.Roles.ImpostorRoles.CamouflagerRole;

[OptionHeader(nameof(English.Lifeboat_Camouflager))]
public sealed class Camouflager : Impostor
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_ImpostorRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Camouflager), "Camouflager")] 
    public static float CamouflagerAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 58),
        HeaderColor = Palette.ImpostorRed,
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(CamouflagerAmount, Alignment.Impostor),
    };
        
    [NumberOption(nameof(English.Lifeboat_Camouflager_GameOptions_Duration), "Camouflage Duration", 10,
        3, 25, 1, false, "{0}s")] 
    public static float CamouflageDuration = 5;
        
    [NumberOption(nameof(English.Lifeboat_Camouflager_GameOptions_Cooldown), "Camouflage Cooldown", 9,
        5, 60, 2.5f, false, "{0:0.0}s")] 
    public static float CamouflageCooldown = 25;

    #endregion

    public override string RoleStringID => nameof(English.Lifeboat_Camouflager);
        
    public override void Start()
    {
        base.Start();
        if (Owner.AmOwner) new CamouflageButton(this);
    }
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Camouflager_IntroText, Color.ToRGBAString(), 
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public void Camouflage(float duration)
    {
        foreach (PlayerControl player in Object.FindObjectsOfType<PlayerControl>())
        {
            AppearanceModification modification = new()
            {
                Data = new AppearanceData
                {
                    Name = "",
                    ColorId = 15,
                    HatId = 0,
                    SkinId = 0,
                    PetId = 0,
                },
                ModificationMask = AppearanceModification.Overrides.Name |
                                   AppearanceModification.Overrides.ColorId |
                                   AppearanceModification.Overrides.HatId |
                                   AppearanceModification.Overrides.SkinId |
                                   AppearanceModification.Overrides.PetId,
                Priority = 99999,
                Timer = duration,
            };

            RoleManager roleManager = player.GetRoleManager();
            if (roleManager && roleManager.AppearanceManager) roleManager.AppearanceManager.Modifications.Add(modification);
        }
    }
}