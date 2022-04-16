using Lifeboat.Buttons;
using Lifeboat.Utils;
using UnityEngine;

namespace Lifeboat.Roles.NeutralRoles.GlitchRole.Buttons;

public sealed class GlitchKillButton : TargetedButton<PlayerControl>
{
    public GlitchKillButton(int offset = 0)
    {
        SetPosition(AspectPosition.EdgeAlignments.RightBottom, shift: offset);
        Cooldown = PlayerControl.GameOptions.KillCooldown;

        if (PlayerControl.GameOptions.MapId == 5) CurrentTime = Cooldown;
        else CurrentTime = 10;
    }

    public override void SetupButtonText()
    {
    }

    public override KeyCode[] Keybinds => new[] {KeyCode.Q};

    public override PlayerControl GetClosest() => UsefulMethods.GetClosestPlayer(TargetRange(), true);

    public override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcMurderPlayer(Target);
    }
}