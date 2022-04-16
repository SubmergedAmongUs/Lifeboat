using Framework.Extensions;
using Framework.Localization;
using HarmonyLib;
using Lifeboat.Extensions;
using Lifeboat.RoleAbilities.GuessAbility.MonoBehaviours;
using Lifeboat.Roles;
using Lifeboat.Roles.CrewmateRoles.SnitchRole;
using Lifeboat.Roles.ImpostorRoles;
using UnityEngine;

namespace Lifeboat.RoleAbilities.GuessAbility.Patches;

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetEnabled))]
public static class PlayerVoteArea_SetEnabled_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerVoteArea __instance)
    {
        GameData.PlayerInfo targetData = GameData.Instance.GetPlayerById(__instance.TargetPlayerId);
        if (__instance.AmDead || !GameData.Instance || targetData == null || targetData.IsDead || targetData.Disconnected) return;
            
        if (!PlayerControl.LocalPlayer.Is(out Impostor impostor) || impostor is not {GuessAbility: { }}) return;
        if (__instance.NameText.text.Contains($"\n<size=70%><color=#{Snitch.Settings.HeaderColor.ToRGBAString()}>{LanguageProvider.Current.Lifeboat_Snitch}")) return;
            
        GameObject buttonObject = new("Guess Button")
        {
            layer = 5
        };

        Transform buttonTransform = buttonObject.transform;
        buttonTransform.parent = __instance.transform;
        buttonTransform.localPosition = new Vector3(0.986f, 0, -3f);
        buttonTransform.localScale = new Vector3(0.7f, 0.7f, 1);

        GuessButton customMeetingButton = buttonObject.AddComponent<GuessButton>();
        customMeetingButton.Parent = __instance;
        customMeetingButton.Impostor = impostor;
    }
}