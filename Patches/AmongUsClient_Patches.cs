using HarmonyLib;
using Lifeboat.Utils;

namespace Lifeboat.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
public static class AmongUsClient_Awake_Patch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        CustomHats.CreateHats();
    }
}
    
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class AmongUsClient_OnGameEnd_Patch
{
    [HarmonyPrefix]
    public static bool Prefix([HarmonyArgument(0)] GameOverReason gameOverReason)
    {
        return gameOverReason != GameOverReason.HumansDisconnect;
    }
}