using System.Collections.Generic;
using Lifeboat.RoleAbilities.SwapAbility;
using Lifeboat.Roles.NeutralRoles.LawyerRole.MonoBehaviours;
using UnityEngine;

namespace Lifeboat.Roles.NeutralRoles.LawyerRole;

public sealed class LawyerSwapAbility : SwapAbility
{
    public Lawyer Lawyer { get; set; }
    public bool WasUsed { get; set; }

    public List<LawyerSwapButton> Buttons { get; set; } = new();

    public LawyerSwapAbility(Lawyer owner, int priority) : base(owner, priority)
    {
        Lawyer = owner;
    }

    public override void Swap(byte firstPlayerId, byte secondPlayerId)
    {
        WasUsed = true;
        base.Swap(firstPlayerId, secondPlayerId);
    }

    public override bool ShouldSpawnButton(PlayerVoteArea voteArea)
    {
        return base.ShouldSpawnButton(voteArea) && Lawyer.TargetSet && !WasUsed && Lawyer.Target is {WasCollected: false, IsDead: false, Disconnected: false}&&
               (voteArea.TargetPlayerId == Lawyer.Target.PlayerId || voteArea.TargetPlayerId == Lawyer.Owner.PlayerId);
    }

    public override void AddButtonComponent(GameObject gameObject, PlayerVoteArea voteArea)
    {
        LawyerSwapButton customMeetingButton = gameObject.AddComponent<LawyerSwapButton>();
        customMeetingButton.Parent = voteArea;
        customMeetingButton.Swapper = this;
    }
}