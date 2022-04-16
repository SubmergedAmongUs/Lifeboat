using Framework.Extensions;
using Framework.Localization.Extensions;
using Framework.Localization.Languages;
using Framework.Utilities;
using Lifeboat.Buttons;
using Lifeboat.Extensions;
using Lifeboat.Roles.NeutralRoles.GlitchRole;
using TMPro;
using UnityEngine;

namespace Lifeboat.RoleModifiers.Modifiers.LoversModifier.Buttons;

public sealed class LoversTextButton : BaseButton
{
    public override bool IsModifierButton => true;
    public override bool CanUseInVents => true;

    public ChatController LoversChat { get; }
        
    public LoversTextButton(ChatController loversChat)
    {
        LoversChat = loversChat;
        Cooldown = CurrentTime = 0.01f;
        SetSprite(ResourceManager.GetSprite("Text", 250));
        SetPosition(AspectPosition.EdgeAlignments.LeftBottom, true, PlayerControl.LocalPlayer.GetRoleManager().MyRole is Glitch ? 2 : 0);
    }
        
    public override void SetupButtonText()
    {
        TextMeshPro text = GameObject.transform.Find("Text_TMP").GetComponent<TextMeshPro>();
        text.GetComponent<TextTranslatorTMP>().DestroyImmediate();
        text.SetLocalizedText(nameof(English.Lifeboat_Lovers_Button_Text));
        text.transform.localPosition = new Vector3(0, -0.3654f, 0);
        text.transform.localScale = new Vector3(0.8f, 0.8f, 1);

        Material material = new(text.fontSharedMaterial);
        material.SetColor("_OutlineColor", new Color32(17, 127, 45, 255));
        text.fontSharedMaterial = material;
    }
        
    public override void OnClick()
    {
        PlayerControl.LocalPlayer.NetTransform.Halt();
        HudManager.Instance.Chat.ForceClosed();
        HudManager.Instance.Chat.SetVisible(false);

        HudManager.Instance.Chat = LoversChat;
            
        HudManager.Instance.Chat.gameObject.SetActive(true);
        HudManager.Instance.Chat.Content.SetActive(true);
        HudManager.Instance.Chat.StartCoroutine(HudManager.Instance.Chat.CoOpen());
    }
}