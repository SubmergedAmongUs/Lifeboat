using HarmonyLib;
using UnityEngine;

namespace Lifeboat.Patches;

[HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.SetCoolDown))]
public static class KillButtonManager_SetCoolDown_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(KillButtonManager __instance, [HarmonyArgument(0)] float timer, [HarmonyArgument(1)] float maxTimer)
    {
        float num = Mathf.Clamp(timer / maxTimer, 0f, 1f);
        if (__instance.renderer) __instance.renderer.material.SetFloat("_Percent", num);
            
        __instance.isCoolingDown = num > 0f;
        if (__instance.isCoolingDown)
        {
            __instance.TimerText.text = Mathf.CeilToInt(timer).ToString();
            __instance.TimerText.gameObject.SetActive(true);
        }
        else
        {
            __instance.TimerText.gameObject.SetActive(false);
        }

        return false;
    }
}