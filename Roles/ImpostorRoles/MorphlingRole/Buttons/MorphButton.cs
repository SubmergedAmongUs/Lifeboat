using Framework.Localization.Languages;
using Hazel;
using Lifeboat.Buttons;
using Lifeboat.Enums;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles.MorphlingRole.Buttons;

public sealed class MorphButton : TargetedButton<PlayerControl>
{
    public MorphButton(Morphling morphling)
    {
        Morphling = morphling;
        SetSprite("Morph", 454.55f);
            
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);

        CurrentTime = Cooldown = Morphling.MorphlingCooldown;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Morphling_Button_Morph));
        SetTextColor(new Color32(228, 0, 194, 255));
    }

    public Morphling Morphling { get; set; }

    public override Color OutlineColor => new Color32(98, 233, 8, 255);
    public override PlayerControl GetClosest() => Morphling.Sampled;

    public override float EffectDuration => Morphling.MorphlingDuration;

    public override void OnClick()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Morphling_Morph, SendOption.Reliable);
        writer.Write(Morphling.Sampled.PlayerId);
        writer.Write(EffectDuration);
        writer.EndMessage();
            
        Morphling.Morph(Morphling.Sampled.PlayerId, EffectDuration);
    }
}