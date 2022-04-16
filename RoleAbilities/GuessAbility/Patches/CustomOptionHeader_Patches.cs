using Framework.CustomOptions.CustomOptions;
using Framework.Localization.Languages;
using HarmonyLib;
using Lifeboat.GameOptions;

namespace Lifeboat.RoleAbilities.GuessAbility.Patches;

[HarmonyPatch(typeof(CustomOptionHeader), nameof(CustomOptionHeader.GetStyledHeaderString))]
public static class CustomOptionHeader_GetSylizedHeaderString_Patch
{
    [HarmonyPrefix]
    public static void Prefix(CustomOptionHeader __instance)
    {
        if (__instance.StringID == nameof(English.Lifeboat_Assassin) && GeneralOptions.AllImpsCanAssassinate) 
            __instance.StringID = nameof(English.Lifeboat_Assassin_UI_Ability);
    }

    [HarmonyPostfix]
    public static void Postfix(CustomOptionHeader __instance)
    {
        if (__instance.StringID == nameof(English.Lifeboat_Assassin_UI_Ability))
            __instance.StringID = nameof(English.Lifeboat_Assassin);
    }
}