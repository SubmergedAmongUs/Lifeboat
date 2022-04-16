using Essentials.PluginLoader.Logging;
using HarmonyLib;
using Mods.Lifeboat.Extensions;
using Mods.Lifeboat.Roles.NeutralRoles.AmnesiacRole.MonoBehaviours;
using UnityEngine;

namespace Mods.Lifeboat.Roles.NeutralRoles.AmnesiacRole.Patches
{
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetEnabled))]
    public static class PlayerVoteArea_SetEnabled_Patch
    {
        public static void Postfix(PlayerVoteArea __instance)
        {
            SubLog.LogAlert(1);
            GameData.PlayerInfo targetData = GameData.Instance!?.GetPlayerById(__instance.TargetPlayerId);
            SubLog.LogAlert(__instance.AmDead);
            SubLog.LogAlert(targetData == null);
            SubLog.LogAlert(!targetData.IsDead);
            SubLog.LogAlert(targetData.Disconnected);

            if (!__instance.AmDead || targetData == null || !targetData.IsDead || targetData.Disconnected) return;
            SubLog.LogAlert(2);
            
            if (!PlayerControl.LocalPlayer.Is(out Amnesiac amne)) return;
            SubLog.LogAlert(3);

            GameObject buttonObject = new("Remember Button")
            {
                layer = 5
            };

            Transform buttonTransform = buttonObject.transform;
            buttonTransform.parent = __instance.transform;
            buttonTransform.localPosition = new Vector3(0.986f, 0, -3f);
            buttonTransform.localScale = new Vector3(0.7f, 0.7f, 1);

            RememberButton customMeetingButton = buttonObject.AddComponent<RememberButton>();
            customMeetingButton.Parent = __instance;
            customMeetingButton.Amnesiac = amne;
        }
    }
}