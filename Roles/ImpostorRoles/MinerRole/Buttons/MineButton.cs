using System.Linq;
using Framework.Utilities;
using Lifeboat.Buttons;
using Submerged.Map.MonoBehaviours;
using UnhollowerBaseLib;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles.MinerRole.Buttons;

public sealed class MineButton : TargetedButton<PlayerControl>
{
    public MineButton(Miner owner)
    {
        Miner = owner;
        SetSprite(ResourceManager.GetSprite("Mine", 250f));

        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        CurrentTime = Cooldown = Miner.MineCooldown;
    }

    public override void SetOutline(PlayerControl component, bool @on = false, Color color = default)
    {
    }

    public Miner Miner { get; set; }
        
    public override PlayerControl GetClosest()
    {
        Il2CppReferenceArray<Collider2D> hits = Physics2D.OverlapBoxAll(PlayerControl.LocalPlayer.transform.position, Miner.VentScale, 0);
        hits = hits.ToArray().Where(c => (c.name.Contains("Vent") || !c.isTrigger) && c.gameObject.layer != 8 && c.gameObject.layer != 5).ToArray();
        bool inElevator = SubmarineStatus.Instance && SubmarineStatus.Instance.Elevators.Any(e => e.GetInElevator(PlayerControl.LocalPlayer));
        if (hits.Count == 0 && !inElevator)
        {
            return PlayerControl.LocalPlayer;
        }
        else
        {
            return null;
        }
    }

    public override void OnClick()
    {
        Miner.RpcPlaceVent(PlayerControl.LocalPlayer.transform.position);
    }
}