using Framework.Localization.Languages;
using Lifeboat.Buttons;
using Lifeboat.Utils;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.SheriffRole.Buttons;

public sealed class SheriffKillButton : TargetedButton<PlayerControl>
{
    public override KeyCode[] Keybinds => new[] {KeyCode.Q};

    public SheriffKillButton(Sheriff sheriff)
    {
        Cooldown = Sheriff.MatchImpostorCooldown ? PlayerControl.GameOptions.KillCooldown : Sheriff.SheriffKillCooldown;
        Sheriff = sheriff;
            
        SetSprite("Shoot", 200);
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
            
        if (PlayerControl.GameOptions.MapId == 5) CurrentTime = Cooldown;
        else CurrentTime = 10;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Sheriff_Button_Shoot));
        SetTextColor(new Color32(196, 150, 69, 255));
    }

    public Sheriff Sheriff { get; set; }
        
    public override void OnClick()
    {
        Sheriff.MurderPlayer(Target);
    }
        
    public override PlayerControl GetClosest() => UsefulMethods.GetClosestPlayer(TargetRange(), false);
}