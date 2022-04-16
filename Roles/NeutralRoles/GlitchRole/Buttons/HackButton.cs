using Framework.Extensions;
using Framework.Localization.Extensions;
using Framework.Localization.Languages;
using Framework.Utilities;
using Lifeboat.Buttons;
using Lifeboat.Utils;
using TMPro;
using UnityEngine;

namespace Lifeboat.Roles.NeutralRoles.GlitchRole.Buttons;

public sealed class HackButton : TargetedButton<PlayerControl>
{
    public HackButton(Glitch owner)
    {
        Glitch = owner;
        SetSprite(ResourceManager.GetSprite("Hack", 250));

        SetPosition(AspectPosition.EdgeAlignments.LeftBottom, true, 1);
        Cooldown = CurrentTime = Glitch.HackCooldown;
    }
        
    public override void SetupButtonText()
    {
        TextMeshPro text = GameObject.transform.Find("Text_TMP").GetComponent<TextMeshPro>();
        text.GetComponent<TextTranslatorTMP>().DestroyImmediate();
        text.SetLocalizedText(nameof(English.Lifeboat_Glitch_Button_Hack));
        text.transform.localPosition = new Vector3(0, -0.3654f, 0);
        text.transform.localScale = new Vector3(0.8f, 0.8f, 1);

        Material material = new(text.fontSharedMaterial);
        material.SetColor("_OutlineColor", Glitch.Settings.HeaderColor);
        text.fontSharedMaterial = material;
    }
        
    public Glitch Glitch { get; set; }

    public override Color OutlineColor => Glitch.Settings.HeaderColor;

    public override PlayerControl GetClosest() => UsefulMethods.GetClosestPlayer(TargetRange(), true);

    public override float EffectDuration { get; set; } = Glitch.HackDuration;

    public override void OnClick()
    {
        Glitch.RpcHack(Target);
    }
}