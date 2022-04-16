using System;
using HarmonyLib;

namespace Lifeboat.Events.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
public static class PlayerControl_Exiled_Patch
{
    [HarmonyPrefix]
    public static void Postfix(PlayerControl __instance)
    {
        PlayerControlEvents.OnPlayerExile?.Invoke(__instance);
    }
}
    
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class PlayerControl_MurderPlayer_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        PlayerControlEvents.OnPlayerMurder?.Invoke(__instance, target);
    }
}