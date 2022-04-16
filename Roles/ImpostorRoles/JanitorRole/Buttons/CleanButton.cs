using Framework.Localization.Languages;
using Framework.Utilities;
using Hazel;
using Lifeboat.Buttons;
using Lifeboat.Enums;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles.JanitorRole.Buttons;

public sealed class CleanButton : TargetedButton<DeadBody>
{
    public CleanButton(Janitor janitor)
    {
        SetSprite(ResourceManager.GetSprite("Clean", 250f));
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        Janitor = janitor;
        Cooldown = CurrentTime = Janitor.CleanCooldown;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Janitor_Button_Clean));
        SetTextColor(new Color32(198, 45, 58, 255));
    }
        
    public Janitor Janitor { get; set; }

    public override Color OutlineColor => new Color32(80, 156, 65, 255);

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
        Collider2D closestHit = null;

        foreach (Collider2D body in Physics2D.OverlapCircleAll(myPosition, PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
        {
            if (!body.CompareTag("DeadBody")) continue;
            float distance = Vector2.Distance(body.transform.position, myPosition);

            if (distance < maxDistance)
            {
                maxDistance = distance;
                closestHit = body;
            }
        }

        if (closestHit) return closestHit.GetComponent<DeadBody>();

        return null;
    }

    public override void OnClick()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Janitor_Clean, SendOption.Reliable);
        writer.Write(Target.ParentId);
        writer.EndMessage();
        if (Janitor.ResetOnClean) PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
        Janitor.Clean(Target.ParentId);
    }
}