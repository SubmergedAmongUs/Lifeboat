using System;
using Framework.Attributes;
using Framework.Extensions;
using Lifeboat.Roles.CrewmateRoles.SwapperRole.MonoBehaviours;
using UnityEngine;

namespace Lifeboat.RoleAbilities.SwapAbility.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class SwapperMeetingBehaviour : MonoBehaviour
{
    public SwapperMeetingBehaviour(IntPtr ptr) : base(ptr) { }
        
    public SwapAbility Swapper;
    public MeetingHud Meeting;
        
    private void Update()
    {
        if (Meeting.state == MeetingHud.VoteStates.Results)
        {
            if (Swapper.Selected.Count == 2) Swapper.RpcSwap();
            enabled = false;
            FindObjectsOfType<BaseSwapButton>().ForEach(s => s.gameObject.SetActive(false));
        }

        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            enabled = false;
            FindObjectsOfType<BaseSwapButton>().ForEach(s => s.gameObject.SetActive(false));
        }
    }
}