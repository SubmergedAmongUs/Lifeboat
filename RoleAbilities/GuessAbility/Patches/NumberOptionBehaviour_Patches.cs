using Framework.CustomOptions.MonoBehaviours.CustomOptionBehaviours;
using Framework.Localization.Languages;
using HarmonyLib;
using Lifeboat.GameOptions;

namespace Lifeboat.RoleAbilities.GuessAbility.Patches;

[HarmonyPatch(typeof(NumberOptionBehaviour), nameof(NumberOptionBehaviour.Update))]
public static class NumberOptionBehaviour_Update_Patch
{
    [HarmonyPrefix]
    public static void Prefix(NumberOptionBehaviour __instance)
    {
        if (__instance.TitleStringID == nameof(English.Lifeboat_Assassin_GameOptions_MaxKillsPerMeeting) && GeneralOptions.AllImpsCanAssassinate)
            __instance.TitleStringID = nameof(English.Lifeboat_Assassin_GameOptions_MaxKillsPerMeetingIndividual);
    }

    [HarmonyPostfix]
    public static void Postfix(NumberOptionBehaviour __instance)
    {
        if (__instance.TitleStringID == nameof(English.Lifeboat_Assassin_GameOptions_MaxKillsPerMeetingIndividual))
            __instance.TitleStringID = nameof(English.Lifeboat_Assassin_GameOptions_MaxKillsPerMeeting);
    }
}