using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Extensions;
using Framework.Utilities;
using Lifeboat.Buttons;
using Lifeboat.Extensions;
using UnityEngine;

namespace Lifeboat.Roles.NeutralRoles.ArsonistRole.Buttons;

public sealed class DouseButton : TargetedButton<PlayerControl>
{
    public DouseButton(Arsonist arsonist)
    {
        Arsonist = arsonist;
        SetSprite(ResourceManager.GetSprite("Douse", 200f));
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        CurrentTime = Cooldown;
    }
        
    public Arsonist Arsonist { get; }

    public override PlayerControl GetClosest()
    {
        IEnumerable<PlayerControl> validPlayers = PlayerControl.AllPlayerControls.ToArray().Where(p => !p.Data.Disconnected && !p.AmOwner && !p.Data.IsDead && Arsonist.DousedPlayers.All(c => c != p.PlayerId));

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

    public override float Cooldown => Arsonist.DouseCooldown;
    public override Color OutlineColor => new Color32(227, 134, 41, 255);

    public override void Update()
    {
        base.Update();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.Data == null || player.Data.IsDead || player.Data.Disconnected || player.AmOwner) continue;
            if (Arsonist.DousedPlayers.Any(p => p == player.PlayerId)) continue;
            return;
        }

        PlayerControl.LocalPlayer.GetRoleManager().StartCoroutine(CoDestroy());
    }

    public IEnumerator CoDestroy()
    {
        yield return null;
        Remove();
        new IgniteButton(Arsonist);
    }
        
    public override void OnClick()
    {
        Arsonist.DousedPlayers.Add(Target.PlayerId);
    }
}