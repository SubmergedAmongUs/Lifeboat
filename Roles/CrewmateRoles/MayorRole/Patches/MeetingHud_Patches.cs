using System.Collections.Generic;
using System.Linq;
using Framework.Localization;
using HarmonyLib;
using Lifeboat.Extensions;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.MayorRole.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingHud_Start_Patch
{
    [HarmonyPrefix]
    public static void Prefix(MeetingHud __instance)
    {
        if (!PlayerControl.LocalPlayer.Is(out Mayor mayor)) return;
        mayor.VoteBank++;
        mayor.VotedThisRound = false;
        mayor.Votes.Clear();
    }
}
    
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingHud_Update_Patch
{
    [HarmonyPostfix]
    public static void Postfix(MeetingHud __instance)
    {
        if (!PlayerControl.LocalPlayer.Is(out Mayor mayor)) return;
        if (!__instance.TimerText.text.Contains("|")) __instance.TimerText.text = 
            string.Format(LanguageProvider.Current.Lifeboat_Mayor_UI_VoteBank, mayor.VoteBank) + " | " + __instance.TimerText.text;
    }
}
    
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Confirm))]
public static class MeetingHud_Confirm_Patch
{
    [HarmonyPrefix]
    public static void Prefix(MeetingHud __instance, [HarmonyArgument(0)] byte suspectStateIdx)
    {
        if (!PlayerControl.LocalPlayer.Is(out Mayor mayor)) return;
        if (suspectStateIdx != 253) mayor.VoteBank--;
        if (!mayor.VotedThisRound)
        {
            mayor.VotedThisRound = true;
            return;
        }
            
        mayor.RpcVote(suspectStateIdx);
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
public static class MeetingHud_PopulateResults_Patch
{
    [HarmonyPostfix]
    public static void Postfix(MeetingHud __instance)
    {
        IEnumerable<Mayor> mayors = PlayerControl.AllPlayerControls.ToArray().Select(p => p.GetRoleManager().MyRole).OfType<Mayor>();
        foreach (Mayor mayor in mayors)
        {
            foreach (byte vote in mayor.Votes)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates.FirstOrDefault(p => p.TargetPlayerId == vote);
                if (!playerVoteArea) continue;

                int idx = 0;
                for (int i = 0; i < playerVoteArea.transform.childCount; i++)
                {
                    Transform child = playerVoteArea.transform.GetChild(i);
                    if (child.name == "playerVote(Clone)") idx++;
                }

                bool state = PlayerControl.GameOptions.AnonymousVotes;
                PlayerControl.GameOptions.AnonymousVotes = true;
                __instance.BloopAVoteIcon(mayor.Owner.Data, idx, playerVoteArea.transform);
                PlayerControl.GameOptions.AnonymousVotes = state;
            }
        }
    }
}
    
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
public static class MeetingHud_Close_Patch
{
    [HarmonyPrefix]
    public static void Prefix(MeetingHud __instance)
    {
        Il2CppSystem.Collections.Generic.Dictionary<byte, int> normal = __instance.CalculateVotes();
        IEnumerable<Mayor> mayors = PlayerControl.AllPlayerControls.ToArray().Select(p => p.GetRoleManager().MyRole).OfType<Mayor>();
        foreach (Mayor mayor in mayors)
        {
            if (!mayor.Owner || mayor.Owner.Data.IsDead || mayor.Owner.Data.Disconnected) continue;
            foreach (byte vote in mayor.Votes)
            {
                if (!normal.ContainsKey(vote)) normal[vote] = 0;
                normal[vote]++;
            }
            mayor.Votes.Clear();
        }

        Il2CppSystem.Collections.Generic.KeyValuePair<byte, int> max = global::Extensions.MaxPair(normal, out bool wasTie);
        __instance.exiledPlayer = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !wasTie && v.PlayerId == max.Key);
    }
}