using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Hazel;
using Lifeboat.Attributes;
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.Roles.ImpostorRoles.SwooperRole.Buttons;

namespace Lifeboat.Roles.ImpostorRoles.SwooperRole;

[OptionHeader(nameof(English.Lifeboat_Swooper))]
public sealed class Swooper : Impostor
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_ImpostorRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Swooper), "Swooper")]
    public static float SwooperAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 52),
        HeaderColor = Palette.ImpostorRed,
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(SwooperAmount, Alignment.Impostor),
    };
        
    [NumberOption(nameof(English.Lifeboat_Swooper_GameOptions_Duration), "Swoop Duration", 10,
        3, 25, 1f, false, "{0}s")] 
    public static float SwoopDuration = 10;
        
    [NumberOption(nameof(English.Lifeboat_Swooper_GameOptions_Cooldown), "Swoop Cooldown", 9,
        5, 60, 2.5f, false, "{0:0.0}s")] 
    public static float SwoopCooldown = 25;

    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Swooper);

    public SwoopButton Button { get; set; }

    public override bool CanUseVents()
    {
        return Button?.CurrentTime != int.MaxValue;
    }
        
    public override void Start()
    {
        base.Start();
        if (Owner.AmOwner) Button = new SwoopButton(this);
    }

    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Swooper_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public void RpcSwoop()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Swooper_Swoop, SendOption.Reliable);
        writer.EndMessage();

        Swoop();
    }

    public void Swoop()
    {
        AppearanceModification modification = new()
        {
            Data = new AppearanceData
            {
                Alpha = PlayerControl.LocalPlayer.GetRoleManager().MyRole.Alignment == Alignment.Impostor ? 0.3f : 0,
            },
            ModificationMask = AppearanceModification.Overrides.Alpha,
            Priority = int.MaxValue,
            Timer = SwoopDuration,
        };
            
        Owner.GetRoleManager().AppearanceManager.Modifications.Add(modification);
    }
}