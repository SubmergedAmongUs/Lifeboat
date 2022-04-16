using System.Collections.Generic;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.RoleAbilities.SwapAbility.Interfaces;
using Lifeboat.Roles;
using Lifeboat.Roles.CrewmateRoles.SwapperRole.MonoBehaviours;
using UnityEngine;

namespace Lifeboat.RoleAbilities.SwapAbility;

public class SwapAbility : BaseAbility
{
    public SwapAbility(BaseRole owner, int priority) : base(owner)
    {
        Priority = priority;
    }
        
    public int Priority { get; }
    public List<ISwapButton> Selected { get; set; } = new();
    public byte SwapOne { get; set; } = byte.MaxValue;
    public byte SwapTwo { get; set; } = byte.MaxValue;

    public void RpcSwap()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Swapper_Swap, SendOption.Reliable);
        writer.WritePacked(Priority);
        writer.Write(Selected[0].Parent.TargetPlayerId);
        writer.Write(Selected[1].Parent.TargetPlayerId);
        writer.EndMessage();
        Swap(Selected[0].Parent.TargetPlayerId, Selected[1].Parent.TargetPlayerId);
    }

    public virtual void Swap(byte firstPlayerId, byte secondPlayerId)
    {
        SwapOne = firstPlayerId;
        SwapTwo = secondPlayerId;
    }

    public virtual bool ShouldSpawnButton(PlayerVoteArea voteArea)
    {
        return !voteArea.AmDead && !PlayerControl.LocalPlayer.Data.IsDead && GameData.Instance.GetPlayerById(voteArea.TargetPlayerId) is {IsDead: false};
    }

    public virtual void AddButtonComponent(GameObject gameObject, PlayerVoteArea voteArea)
    {
        SwapButton customMeetingButton = gameObject.AddComponent<SwapButton>();
        customMeetingButton.Parent = voteArea;
        customMeetingButton.Swapper = this;
    }
}