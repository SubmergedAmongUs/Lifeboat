using System.Collections.Generic;
using Framework.Extensions;
using Lifeboat.Extensions;
using UnityEngine;

namespace Lifeboat.Buttons;

public abstract class TargetedButton<T> : BaseButton where T : Component
{
    public virtual bool ShowOutlineDuringEffect { get; set; }
    public virtual Color OutlineColor { get; set; } = Color.yellow;
    public virtual Color OutlineColorEffect => OutlineColor;
    public T Target { get; set; }

    protected TargetedButton()
    {
        ClickEvent.RemoveAllListeners();
        ClickEvent.AddListener((System.Action) (() =>
        {
            if (!CanUse() || CurrentTime > 0) return;
                
            UpdateTarget();
            if (!Target)
            {
                return;
            }
                
            SetOutline(Target);
                
            CurrentTime = Cooldown;
            if (EffectDuration > 0)
            {
                EffectCoroutine = PlayerControl.LocalPlayer.GetRoleManager().StartCoroutine(ShowEffectDuration(EffectDuration));
            }
                
            OnClick();
        }));
    }
        
    public override void DoWhileEffect() => SetOutline(Target, ShowOutlineDuringEffect, Color.green);
        
    public void UpdateTarget()
    {
        T newTarget = GetClosest();
        if (Target != newTarget)
        {
            SetOutline(Target);
            SetOutline(newTarget, true, OutlineColor);
            Target = newTarget;
        }
    }
        
    public override void Update()
    {
        base.Update();
        if (GameObject.active && CurrentTime != int.MaxValue)
        {
            UpdateTarget();
                
            KillButtonManager.renderer.color = CanUse() && Target ? Palette.EnabledColor : Palette.DisabledClear;
            KillButtonManager.renderer.material.SetFloat("_Desat", Target ? 0 : 1);
            KillButtonManager.killText.alpha = CanUse() && Target ? Palette.EnabledColor.a : Palette.DisabledClear.a;
        }
    }
        
    public virtual float TargetRange() => GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];

    public virtual T GetClosest()
    {
        IEnumerable<T> possibleTargets = GameObject.FindObjectsOfType<T>();

        float maxDistance = TargetRange();
        Vector2 myPos = PlayerControl.LocalPlayer.GetTruePosition();

        T result = null;
        foreach (T item in possibleTargets)
        {
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
        
    public virtual void SetOutline(T component, bool on = false, Color color = default)
    {
        if (!component) return;

        SpriteRenderer rend = component.GetComponentInChildren<SpriteRenderer>();
        rend.material.SetFloat("_Outline", on ? 1 : 0);
        if (on) rend.material.SetColor("_OutlineColor", color);
    }

    public override void Remove()
    {
        SetOutline(Target, false);
        base.Remove();
    }
}