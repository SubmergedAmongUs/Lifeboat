using System.Linq;
using Framework.Extensions;
using HarmonyLib;
using Lifeboat.Extensions;

namespace Lifeboat.Patches;

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
public static class IntroCutscene_BeginCrewmate_Patch
{
    [HarmonyPrefix]
    public static void Prefix([HarmonyArgument(0)] out Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        yourTeam = PlayerControl.LocalPlayer.GetRoleManager().GetIntroTeam().OrderBy(p => p == PlayerControl.LocalPlayer ? 0 : 1).ToList().ToIl2CppList();
    }

    [HarmonyPostfix]
    public static void Postfix(IntroCutscene __instance)
    {
        PlayerControl.LocalPlayer.GetRoleManager().SetIntroAppearance(__instance);
    }
}

[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
public static class IntroCutscene_BeginImpostor_Patch
{
    [HarmonyPrefix]
    public static void Prefix([HarmonyArgument(0)] out Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        yourTeam = PlayerControl.LocalPlayer.GetRoleManager().GetIntroTeam().OrderBy(p => p == PlayerControl.LocalPlayer ? 0 : 1).ToList().ToIl2CppList();
    }

    [HarmonyPostfix]
    public static void Postfix(IntroCutscene __instance)
    {
        PlayerControl.LocalPlayer.GetRoleManager().SetIntroAppearance(__instance);
    }
}