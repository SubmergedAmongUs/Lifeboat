using Framework.Extensions;
using HarmonyLib;
using Lifeboat.Extensions;
using Lifeboat.RoleAbilities.GuessAbility.MonoBehaviours;
using Lifeboat.Roles;
using Lifeboat.Roles.ImpostorRoles;
using UnityEngine;

namespace Lifeboat.RoleAbilities.GuessAbility.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
public static class MeetingHud_PopulateResults_Patch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (!PlayerControl.LocalPlayer.Is(out Impostor impostor) || impostor is not {GuessAbility: { } ability}) return;
            
        GameObject.FindObjectsOfType<GuessButton>().ForEach(g => g.gameObject.Destroy());
        if (ability.Overlay)
        {
            ability.Overlay.Destroy();
            ability.Overlay = null;
            ability.TargetPlayer = null;
        }
    }
}