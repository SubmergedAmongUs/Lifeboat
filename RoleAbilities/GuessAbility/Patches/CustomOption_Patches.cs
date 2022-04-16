using Framework.CustomOptions.CustomOptions;
using Framework.Localization.Languages;
using HarmonyLib;
using Lifeboat.GameOptions;

namespace Lifeboat.RoleAbilities.GuessAbility.Patches;

[HarmonyPatch(typeof(CustomOption), nameof(CustomOption.GetHudString))]
public static class CustomOption_GetHudString_Patch
{
    [HarmonyPrefix]
    public static void Prefix(CustomOption __instance)
    {
        if (__instance.StringID == nameof(English.Lifeboat_Assassin_GameOptions_MaxKillsPerMeeting) && GeneralOptions.AllImpsCanAssassinate)
            __instance.StringID = nameof(English.Lifeboat_Assassin_GameOptions_MaxKillsPerMeetingIndividual);
    }

    [HarmonyPostfix]
    public static void Postfix(CustomOption __instance)
    {
        if (__instance.StringID == nameof(English.Lifeboat_Assassin_GameOptions_MaxKillsPerMeetingIndividual))
            __instance.StringID = nameof(English.Lifeboat_Assassin_GameOptions_MaxKillsPerMeeting);
    }
}