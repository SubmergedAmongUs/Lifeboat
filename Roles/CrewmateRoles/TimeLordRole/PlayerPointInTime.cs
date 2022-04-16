using Lifeboat.Utils;
using Submerged.Map.MonoBehaviours;
using Submerged.Systems.CustomSystems.PlayerFloor;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.TimeLordRole;

public readonly struct PlayerPointInTime
{
    public PlayerPointInTime(PlayerControl player, float time)
    {
        Time = time;
        PlayerId = player.PlayerId;
        Floor = SubmarineStatus.Instance && SubmarinePlayerFloorSystem.Instance.PlayerFloorStates.TryGetValue(PlayerId, out bool floor) && floor;
        Alive = !player.Data.IsDead;
        Velocity = player.MyPhysics.body.velocity;
        Position = player.transform.position;
        TargetPosition = player.NetTransform.targetSyncPosition;
        TargetVelocity = player.NetTransform.targetSyncVelocity;
    }

    public void Rewind(PlayerControl owner)
    {
        if (owner.inVent) return;
        if (SubmarineStatus.Instance) SubmarinePlayerFloorSystem.Instance.PlayerFloorStates[PlayerId] = Floor;
        if (owner.Data.IsDead && Alive) ReviveHandler.ReviveAndRemoveBody(owner);
        else if (!owner.Data.IsDead && !Alive) CustomMurders.MurderNoAnim(owner, owner);
        owner.MyPhysics.body.velocity = Velocity;
        owner.transform.position = Position;
        owner.NetTransform.targetSyncPosition = TargetPosition;
        owner.NetTransform.targetSyncVelocity = TargetVelocity;
    }

    public readonly float Time;
    private readonly byte PlayerId;
    private readonly bool Floor;
    private readonly bool Alive;
    private readonly Vector2 Velocity;
    private readonly Vector2 Position;
    private readonly Vector2 TargetPosition;
    private readonly Vector2 TargetVelocity;
}