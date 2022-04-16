using Framework.Extensions;
using Framework.Localization.Languages;
using Lifeboat.Buttons;
using Lifeboat.Extensions;
using Lifeboat.Utils;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.EngineerRole.Buttons;

public sealed class FixButton : TargetedButton<PlayerControl>
{
    public FixButton(Engineer engineer)
    {
        Engineer = engineer;
        SetSprite("Fix", 200);
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        CurrentTime = 0;
        Uses = (int) Engineer.FixAmount;
    }

    public Engineer Engineer { get; }
    public bool IsUseAvailable { get; set; } = true;
    public int Uses { get; set; }

    public override bool CanUseInVents => true;

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Engineer_Button_Fix));
        SetTextColor(Engineer.Settings.HeaderColor);
    }

    public override void SetOutline(PlayerControl component, bool on = false, Color color = default) { }

    public override PlayerControl GetClosest() => SabotageUtils.AnyActive() && IsUseAvailable ? PlayerControl.LocalPlayer : null;

    public override void OnClick()
    {
        SabotageUtils.FixAllSabotages();
        IsUseAvailable = Engineer.CanFixMultipleTimesPerRound;
        Uses--;

        if (Uses == 0)
        {
            GameObject.Destroy();
            if (Engineer.Button == this) Engineer.Button = null;
            PlayerControl.LocalPlayer.GetRoleManager().Buttons.Remove(this);
        }
    }
}