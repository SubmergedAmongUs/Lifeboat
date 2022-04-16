using System.Collections;
using Framework.Extensions;
using HarmonyLib;
using UnityEngine;

namespace Lifeboat.Debugging;

[HarmonyPatch(typeof(DummyBehaviour), nameof(DummyBehaviour.Update))]
public static class DummyBehaviour_Update_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(DummyBehaviour __instance)
    {
        if (!LifeboatDebug.Instance.MakeDummiesVoteForMe) return true;

        GameData.PlayerInfo data = __instance.myPlayer.Data;
        if (data == null || data.IsDead)
        {
            return false;
        }

        if (MeetingHud.Instance)
        {
            if (!__instance.voted)
            {
                __instance.voted = true;
                __instance.StopAllCoroutines();
                __instance.StartCoroutine(NewDoVote(__instance));
                return false;
            }
        }
        else
        {
            __instance.voted = false;
        }

        return false;
    }

    public static IEnumerator NewDoVote(DummyBehaviour __instance)
    {
        yield return new WaitForSeconds(20f);
        MeetingHud.Instance.CmdCastVote(__instance.myPlayer.PlayerId, PlayerControl.LocalPlayer.PlayerId);
    }
}