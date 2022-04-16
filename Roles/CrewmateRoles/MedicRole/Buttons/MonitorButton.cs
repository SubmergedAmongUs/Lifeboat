using Framework.Localization.Languages;
using Lifeboat.Buttons;
using Lifeboat.Extensions;
using Lifeboat.Utils;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.MedicRole.Buttons;

public sealed class MonitorButton : TargetedButton<PlayerControl>
{
    public MonitorButton(Medic owner)
    {
        Medic = owner;
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        SetSprite("Monitor", 454.55f);
        CurrentTime = Cooldown;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Medic_Button_Monitor));
        SetTextColor(Color.green);
    }

    public Medic Medic { get; set; }

    public override float Cooldown => Medic.ShieldCooldown;

    public override Color OutlineColor => Color.green;

    public override PlayerControl GetClosest() => GameData.Instance.GetPlayerById(Medic.Protected) is {IsDead: false, Disconnected: false} && !Medic.CanSwapShield 
        ? null : UsefulMethods.GetClosestPlayer(TargetRange(), false, Medic.Protected);

    public override void OnClick()
    {
        Medic.RpcProtect(Target);
        SetOutline(Target);
        if (Medic.ShieldPer > 0)
        {
            Object.Destroy(GameObject);
            PlayerControl.LocalPlayer.GetRoleManager().Buttons.Remove(this);
        }
    }
}