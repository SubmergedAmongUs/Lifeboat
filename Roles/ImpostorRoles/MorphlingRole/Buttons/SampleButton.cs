using Framework.Localization.Languages;
using Lifeboat.Buttons;
using Lifeboat.Utils;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles.MorphlingRole.Buttons;

public sealed class SampleButton : TargetedButton<PlayerControl>
{
    public SampleButton(Morphling morphling)
    {
        SetPosition(AspectPosition.EdgeAlignments.LeftBottom);
        SetSprite("Sample", 454.55f);
        Morphling = morphling;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Morphling_Button_Sample));
        SetTextColor(new Color32(228, 0, 194, 255));
    }

    public Morphling Morphling { get; set; }

    public override float Cooldown { get; set; } = 5f;
    public override float CurrentTime { get; set; } = 5f;
    public override Color OutlineColor => Color.magenta;

    public override PlayerControl GetClosest() => UsefulMethods.GetClosestPlayer(TargetRange(), false);

    public override void OnClick()
    {
        Morphling.Sampled = Target;
    }
}