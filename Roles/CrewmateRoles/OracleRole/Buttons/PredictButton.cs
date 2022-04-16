using Framework.Localization.Languages;
using Lifeboat.Buttons;
using Lifeboat.Extensions;
using Lifeboat.Utils;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.OracleRole.Buttons;

public sealed class PredictButton : TargetedButton<PlayerControl>
{
    public PredictButton(Oracle owner)
    {
        Oracle = owner;
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        SetSprite("Predict", 454.55f);
        CurrentTime = Cooldown = Oracle.PredictCooldown;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Oracle_Button_Predict));
        SetTextColor(Color.blue);
    }

    public Oracle Oracle { get; }

    public override Color OutlineColor => Color.blue;

    public override PlayerControl GetClosest() => UsefulMethods.GetClosestPlayer(TargetRange(), false);

    public override void OnClick()
    {
        Oracle.RpcPredictPlayer(Target.PlayerId);
        SetOutline(Target);
        Object.Destroy(GameObject);
        PlayerControl.LocalPlayer.GetRoleManager().Buttons.Remove(this);
    }
}