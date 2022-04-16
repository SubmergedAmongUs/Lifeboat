using HarmonyLib;
using Lifeboat.Extensions;

namespace Lifeboat.Roles.CrewmateRoles.MayorRole.Patches;

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
public static class PlayerVoteArea_Select_Patch
{
    [HarmonyPrefix]
    public static void Prefix(PlayerVoteArea __instance, out bool __state)
    {
        __state = __instance.voteComplete;
        if (!PlayerControl.LocalPlayer.Is(out Mayor mayor)) return;
        __instance.voteComplete = mayor.VoteBank == 0 || MeetingHud.Instance.state >= MeetingHud.VoteStates.Results;
    }

    [HarmonyPostfix]
    public static void Postfix(PlayerVoteArea __instance, bool __state)
    {
        if (!PlayerControl.LocalPlayer.Is(out Mayor _)) return;
        __instance.voteComplete = __state;
    }
}
    
[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.VoteForMe))]
public static class PlayerVoteArea_VoteForMe_Patch
{
    [HarmonyPrefix]
    public static void Prefix(PlayerVoteArea __instance, out bool __state)
    {
        __state = __instance.voteComplete;
        if (!PlayerControl.LocalPlayer.Is(out Mayor mayor)) return;
        __instance.voteComplete = mayor.VoteBank == 0 || MeetingHud.Instance.state >= MeetingHud.VoteStates.Results;
    }

    [HarmonyPostfix]
    public static void Postfix(PlayerVoteArea __instance, bool __state)
    {
        if (!PlayerControl.LocalPlayer.Is(out Mayor _)) return;
        __instance.voteComplete = __state;
    }
}