using HarmonyLib;

namespace Lifeboat.RoleModifiers.Modifiers.LoversModifier.Patches;

[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.SetVisible))]
public static class BanMenu_SetVisible_Patch
{
    [HarmonyPrefix]
    public static void Prefix(BanMenu __instance, [HarmonyArgument(0)] ref bool visible)
    {
        visible &= HudManager.Instance.Chat.name != "LoversChatController";
    }
}