using System.Collections.Generic;
using System.Linq;
using Framework.Extensions;
using Framework.Localization.Languages;
using Framework.Utilities;
using Lifeboat.Buttons;
using Lifeboat.Extensions;
using Lifeboat.Roles.CrewmateRoles.AltruistRole;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles.UndertakerRole.Buttons;

public sealed class DragDropButton : TargetedButton<DeadBody>
{
    public DeadBody DropTarget;
        
    public List<Altruist> Altruists = new();
    public string CurrentTranslation;
        
    public DragDropButton(Undertaker undertaker, int shift = -1)
    {
        GameObject targetObj = new("Drop Target");
        targetObj.SetActive(false);
        targetObj.transform.SetParent(GameObject.transform);
        DropTarget = targetObj.AddComponent<DeadBody>();

        Undertaker = undertaker;
        SetSprite(ResourceManager.SpriteCache["DragDrop"]);
            
        SetPosition(AspectPosition.EdgeAlignments.RightBottom, true, shift);
        CurrentTime = Cooldown = Undertaker.DragCooldown;

        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            if (playerControl.IsThere() && playerControl.Data is {Disconnected: false} && playerControl.GetRoleManager()!?.MyRole is Altruist altruist)
            {
                if (!Altruists.Contains(altruist)) Altruists.Add(altruist);
            }
        }
    }
        
    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(CurrentTranslation = nameof(English.Lifeboat_Undertaker_Button_Drag));
        SetTextColor(new Color32(215, 30, 34, 255));
    }
        
    public Undertaker Undertaker { get; set; }

    public override void OnClick()
    {
        if (Undertaker.Corpse.IsThere())
        {
            Undertaker.RpcDropBody(Undertaker.Corpse.Parent);
        }
        else
        {
            Undertaker.RpcDragBody(Target);
            CurrentTime = 0.1f;
        }
    }

    public override void Update()
    {
        base.Update();
            
        bool corpseIsThere = Undertaker.Corpse.IsThere();
        if (Undertaker.Corpse is not null && !corpseIsThere) Undertaker.Corpse = null;

        if (CurrentTranslation == nameof(English.Lifeboat_Undertaker_Button_Drag) && corpseIsThere)
        {
            SetTextTranslation(CurrentTranslation = nameof(English.Lifeboat_Undertaker_Button_Drop));
        }
        else if (CurrentTranslation == nameof(English.Lifeboat_Undertaker_Button_Drop) && !corpseIsThere)
        {
            SetTextTranslation(CurrentTranslation = nameof(English.Lifeboat_Undertaker_Button_Drag));
        }
    }

    public override Color OutlineColor => Color.red;

    public override DeadBody GetClosest()
    {
        if (Undertaker.Corpse.IsThere()) return DropTarget;
            
        IEnumerable<DeadBody> possibleTargets = GameObject.FindObjectsOfType<DeadBody>();

        float maxDistance = TargetRange();
        Vector2 myPos = PlayerControl.LocalPlayer.GetTruePosition();

        DeadBody result = null;
        foreach (DeadBody item in possibleTargets)
        {
            if (Altruists.Where(a => !a.IsDestroyed && a.IsReviving != byte.MaxValue)
                .SelectMany(a => new[] { a.Owner.PlayerId, a.IsReviving })
                .Contains(item.ParentId)) continue;
                
            Vector2 deltaVector2 = (Vector2) item.transform.position - myPos;
            float magnitude = deltaVector2.magnitude;
            if (magnitude <= maxDistance && !PhysicsHelpers.AnyNonTriggersBetween(myPos, deltaVector2.normalized, magnitude, Constants.ShipAndObjectsMask))
            {
                result = item;
                maxDistance = magnitude;
            }
        }

        return result;
    }

    public override void SetOutline(DeadBody component, bool on = false, Color color = default)
    {
        if (Undertaker.Corpse.IsThere() || !component.IsThere() || !component.bodyRenderer.IsThere()) return;
        component.bodyRenderer.material.SetFloat("_Outline", on ? 1 : 0);
        if (on) component.bodyRenderer.material.SetColor("_OutlineColor", color);
    }
}