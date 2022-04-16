using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Localization.Languages;
using Framework.Utilities;
using Hazel;
using Lifeboat.Buttons;
using Lifeboat.Enums;
using Lifeboat.Roles.ImpostorRoles.BomberRole;
using TMPro;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles.PoisonerRole.Buttons;

public sealed class PoisonButton : TargetedButton<PlayerControl>
{
    public Poisoner Poisoner { get; }

    public PoisonButton(Poisoner poisoner)
    {
        SetSprite(ResourceManager.GetSprite("Poison", 200));
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        Poisoner = poisoner;
        Cooldown = PlayerControl.GameOptions.KillCooldown;
            
        if (PlayerControl.GameOptions.MapId == 5) CurrentTime = Cooldown;
        else CurrentTime = 10;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Poisoner_Button_Poison));
        SetTextColor(Poisoner.Settings.HeaderColor);
    }
        
    public override float EffectDuration => Poisoner.KillDelay;
    public override bool ShowOutlineDuringEffect => true;

    public override void DoWhileEffect() => SetOutline(Target, ShowOutlineDuringEffect, Poisoner.Settings.HeaderColor);

    public override PlayerControl GetClosest()
    {
        IEnumerable<PlayerControl> validPlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => !p.Data.Disconnected && !p.AmOwner && !p.Data.IsDead && !p.Data.IsImpostor);

        float maxDistance = TargetRange();
        Vector2 myPos = PlayerControl.LocalPlayer.GetTruePosition();

        PlayerControl result = null;
        foreach (PlayerControl player in validPlayers)
        {
            Vector2 deltaVector2 = player.GetTruePosition() - myPos;
            float magnitude = deltaVector2.magnitude;
            if (magnitude <= maxDistance && !PhysicsHelpers.AnyNonTriggersBetween(myPos, deltaVector2.normalized, magnitude, Constants.ShipAndObjectsMask))
            {
                result = player;
                maxDistance = magnitude;
            }
        }

        return result;
    }

    public override void OnClick()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRpcCalls.Poisoner_Poison, SendOption.Reliable);
        writer.Write(Target.PlayerId);
        writer.EndMessage();

        Poisoner.Poison(Target);
        PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.GameOptions.KillCooldown + EffectDuration);
    }

    public override IEnumerator ShowEffectDuration(float duration)
    {
        yield return base.ShowEffectDuration(duration);
            
        CurrentTime = Cooldown;
        PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
    }
}