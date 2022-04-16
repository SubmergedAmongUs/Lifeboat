using System.Linq;
using HarmonyLib;
using Lifeboat.Extensions;
using UnityEngine;

namespace Lifeboat.RoleAbilities.SwapAbility.Patches;

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetEnabled))]
public static class PlayerVoteArea_SetEnabled_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerVoteArea __instance)
    {
        if (!GameData.Instance || PlayerControl.LocalPlayer.GetRoleManager().MyRole.Abilities.OfType<SwapAbility>().FirstOrDefault() is not { } swapper) return;
        if (!swapper.ShouldSpawnButton(__instance)) return;
            
        GameObject buttonObject = new("Swap Button")
        {
            layer = 5
        };

        Transform buttonTransform = buttonObject.transform;
        buttonTransform.parent = __instance.transform;
        buttonTransform.localPosition = new Vector3(0.986f, 0, -3f);
        buttonTransform.localScale = new Vector3(0.7f, 0.7f, 1);

        swapper.AddButtonComponent(buttonObject, __instance);
    }
}