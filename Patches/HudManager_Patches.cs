using HarmonyLib;

namespace Lifeboat.Patches;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
public static class HudManager_SetHudActive_Patch
{
    [HarmonyPostfix]
    public static void Postfix(HudManager __instance)
    {
        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            __instance.ReportButton.gameObject.SetActive(false);
            __instance.KillButton.gameObject.SetActive(false);
        }
    }
}