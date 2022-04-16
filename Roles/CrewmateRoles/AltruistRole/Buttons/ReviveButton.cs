using Framework.Extensions;
using Framework.Localization.Languages;
using Framework.Utilities;
using Hazel;
using Lifeboat.Buttons;
using Lifeboat.Enums;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.AltruistRole.Buttons;

public sealed class ReviveButton : TargetedButton<DeadBody>
{
    public ReviveButton(Altruist altruist)
    {
        SetSprite(ResourceManager.GetSprite("Revive", 250f));
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        Altruist = altruist;
            
        OutlineColor = new Color32(68, 224, 196, 255);
        CurrentTime = 0;
        EffectDuration = Altruist.ReviveDuration;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Altruist_Button_Revive));
        SetTextColor(new Color32(68, 224, 196, 255));
    }
        
    public Altruist Altruist { get; set; }
        
    public override void SetOutline(DeadBody component, bool @on = false, Color color = default)
    {
        if (!component) return;
        component.bodyRenderer.material.SetFloat("_Outline", on ? 1 : 0);
        if (on) component.bodyRenderer.material.SetColor("_OutlineColor", color);
    }
        
    public override DeadBody GetClosest()
    {
        Vector2 myPosition = PlayerControl.LocalPlayer.GetTruePosition();
            
        float maxDistance = TargetRange();
        DeadBody closestHit = null;
 
        foreach (Collider2D body in Physics2D.OverlapCircleAll(myPosition, PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
        {
            if (!body.CompareTag("DeadBody")) continue;
                
            float distance = Vector2.Distance(body.transform.position, myPosition);
            DeadBody deadBody = body.GetComponent<DeadBody>();
            GameData.PlayerInfo bodyData = GameData.Instance.GetPlayerById(deadBody.ParentId);
            if (distance < maxDistance && bodyData is {Disconnected: false})
            {
                maxDistance = distance;
                closestHit = deadBody;
            }
        }

        if (closestHit) return closestHit.GetComponent<DeadBody>();
            
        return null;
    }

    public override void OnClick()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Altruist_Revive, SendOption.Reliable);
        writer.Write(Target.ParentId);
        writer.Write(Target.transform.position);
        writer.EndMessage();
            
        Altruist.RevivePlayer(Target.ParentId, Target.transform.position);
    }
}