using Framework.Localization.Languages;
using Hazel;
using Lifeboat.Buttons;
using Lifeboat.Enums;
using UnityEngine;

namespace Lifeboat.Roles.NeutralRoles.GlitchRole.Buttons;

public sealed class MimicButton : TargetedButton<PlayerControl>
{
    public MimicButton(Glitch glitch)
    {
        Glitch = glitch;
        SetSprite("Mimic", 200);
        SetPosition(AspectPosition.EdgeAlignments.RightBottom, true, 1);
        Cooldown = CurrentTime = Glitch.MorphCooldown;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Glitch_Button_Mimic));
        SetTextColor(Glitch.Settings.HeaderColor);
    }

    public Glitch Glitch { get; set; }

    public override Color OutlineColor => Glitch.Settings.HeaderColor;
    public override PlayerControl GetClosest() => Glitch.Sampled;

    public override float EffectDuration => Glitch.MorphDuration;

    public override void OnClick()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Glitch_Morph, SendOption.Reliable);
        writer.Write(Glitch.Sampled.PlayerId);
        writer.Write(EffectDuration);
        writer.EndMessage();
            
        Glitch.Morph(Glitch.Sampled.PlayerId, EffectDuration);
    }
}