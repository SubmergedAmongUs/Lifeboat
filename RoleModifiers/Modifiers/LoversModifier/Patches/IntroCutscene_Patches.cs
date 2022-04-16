using HarmonyLib;
using Lifeboat.Extensions;
using TMPro;
using UnityEngine;

namespace Lifeboat.RoleModifiers.Modifiers.LoversModifier.Patches;

[HarmonyPatch(typeof(IntroCutscene._CoBegin_d__14), nameof(IntroCutscene._CoBegin_d__14.MoveNext))]
public static class IntroCutscene_CoBegin_Patch
{
    [HarmonyPrefix]
    public static void Prefix(IntroCutscene._CoBegin_d__14 __instance)
    {
        if (__instance.__1__state != 0) return;
        if (PlayerControl.LocalPlayer.GetRoleManager().MyModifier is not Lovers lovers) return;

        TextMeshPro origText = __instance.__4__this.ImpostorText;
        TextMeshPro loverText = GameObject.Instantiate(origText, origText.transform.parent, true);
        loverText.transform.position = new Vector3(origText.transform.position.x, origText.transform.position.y -3.345501f, origText.transform.position.z);
        loverText.text = lovers.GetImportantTaskText();
    }
}