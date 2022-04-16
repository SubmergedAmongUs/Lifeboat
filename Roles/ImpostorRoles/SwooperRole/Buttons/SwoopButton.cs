using Framework.Localization.Languages;
using Lifeboat.Buttons;

namespace Lifeboat.Roles.ImpostorRoles.SwooperRole.Buttons;

public sealed class SwoopButton : BaseButton
{
    public SwoopButton(Swooper swooper)
    {
        Swooper = swooper;
        
        SetSprite("Swoop", 200);
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        CurrentTime = Cooldown = Swooper.SwoopCooldown;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Swooper_Button_Swoop));
        SetTextColor(Palette.ImpostorRed);
    }

    public override float EffectDuration { get; set; } =  Swooper.SwoopDuration;
    public Swooper Swooper;
        
    public override void OnClick()
    {
        Swooper.RpcSwoop();
    }
}