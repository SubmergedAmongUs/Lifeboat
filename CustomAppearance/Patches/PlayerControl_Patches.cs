using HarmonyLib;
using Lifeboat.Extensions;

namespace Lifeboat.CustomAppearance.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetColor))]
public static class PlayerControl_SetColor_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] int bodyColor)
    {
        __instance.GetAppearanceManager()?.SetColor(bodyColor);
    }
}
    
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetHat))]
public static class PlayerControl_SetHat_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] uint hatId, [HarmonyArgument(1)] int colorId)
    {
        __instance.GetAppearanceManager()?.SetHat(hatId, colorId);
    }
}
    
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetSkin))]
public static class PlayerControl_SetSkin_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] uint skinId)
    {
        __instance.GetAppearanceManager()?.SetSkin(skinId);
    }
}
    
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetPet))]
public static class PlayerControl_SetPet_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] uint petId)
    {
        AppearanceManager appearanceManager = __instance.GetAppearanceManager();
        if (!appearanceManager) return;

        int colorId = __instance.Data?.ColorId ?? 0 ;
        appearanceManager.SetPet(petId, colorId);
    }
}