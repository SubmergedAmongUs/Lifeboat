using System.Linq;
using Framework.Extensions;
using HarmonyLib;
using Lifeboat.Extensions;
using UnityEngine;

namespace Lifeboat.CustomAppearance.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingHud_Update_Patch
{
    [HarmonyPostfix]
    public static void Postfix(MeetingHud __instance)
    {
        foreach (PlayerVoteArea voteArea in __instance.playerStates)
        {
            voteArea.NameText.color = Color.white;
        }

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            AppearanceManager appearanceManager = player.GetAppearanceManager();
            if (!appearanceManager) continue;

            PlayerVoteArea myState = __instance.playerStates.FirstOrDefault(p => p.TargetPlayerId == player.PlayerId);
            if (!myState) continue;

            if (!player.Data.Disconnected && appearanceManager.RoleManager.IsThere())
            {
                string text = player.Data.PlayerName;
                foreach (NameOverride nameModifier in appearanceManager.RoleManager.NameOverrides)
                {
                    text = nameModifier.Modifier?.Invoke(player, text, true);
                }
                myState.NameText.text = text;
            }
        }
    }
}