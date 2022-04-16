using Framework.Localization.Languages;
using Framework.Utilities;
using Hazel;
using Lifeboat.Buttons;
using Lifeboat.Enums;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles.CamouflagerRole.Buttons;

public sealed class CamouflageButton : BaseButton
{
    public CamouflageButton(Camouflager camouflager)
    {
        Camouflager = camouflager;
        SetSprite(ResourceManager.GetSprite("Camouflage", 200));

        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        CurrentTime = Cooldown;
    }
        
    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Camouflager_Button_Camouflage));
        SetTextColor(new Color32(0, 100, 0, 255));
    }

    public override float Cooldown => Camouflager.CamouflageCooldown;
    public Camouflager Camouflager { get; set; }

    public override float EffectDuration => Camouflager.CamouflageDuration;

    public override void OnClick()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Camouflager_Camouflage, SendOption.Reliable);
        writer.Write(EffectDuration);
        writer.EndMessage();
            
        Camouflager.Camouflage(EffectDuration);
    }
}