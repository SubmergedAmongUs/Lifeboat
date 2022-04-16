using Framework.Localization.Languages;
using Lifeboat.Buttons;
using Lifeboat.Utils;
using UnityEngine;

namespace Lifeboat.Roles.NeutralRoles.GlitchRole.Buttons;

public sealed class GlitchSampleButton : TargetedButton<PlayerControl>
{
    public GlitchSampleButton(Glitch glitch)
    {
        SetSprite("GlitchSample", 200);
        SetPosition(AspectPosition.EdgeAlignments.LeftBottom);

        Glitch = glitch;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Glitch_Button_Sample));
        SetTextColor(Glitch.Settings.HeaderColor);
    }

    public Glitch Glitch { get; set; }

    public override float Cooldown { get; set; } = 5f;
    public override float CurrentTime { get; set; } = 5f;
    public override Color OutlineColor => Glitch.Settings.HeaderColor;

    public override PlayerControl GetClosest() => UsefulMethods.GetClosestPlayer(TargetRange(), false);

    public override void OnClick()
    {
        Glitch.Sampled = Target;
    }
}