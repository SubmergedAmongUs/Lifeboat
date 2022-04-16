using Framework.Localization.Languages;
using Framework.Utilities;
using Hazel;
using Lifeboat.Buttons;
using Lifeboat.Enums;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.TimeLordRole.Buttons;

public sealed class RewindButton : BaseButton
{
    public RewindButton(TimeLord timeLord)
    {
        TimeLord = timeLord;
        SetSprite(ResourceManager.GetSprite("Rewind", 200));
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        CurrentTime = Cooldown = TimeLord.RewindCooldown;
    }
        
    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_TimeLord_Button_Rewind));
        SetTextColor(new Color32(0, 138, 243, 255));
    }
        
    public TimeLord TimeLord { get; set; }

    public override void OnClick()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.TimeLord_Rewind, SendOption.Reliable);
        writer.EndMessage();

        RewindTime.StartRewind(TimeLord);
    }
}