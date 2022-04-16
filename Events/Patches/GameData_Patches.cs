using HarmonyLib;
using InnerNet;

namespace Lifeboat.Events.Patches;

[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
public static class GameData_HandleDisconnect_Patch
{
    [HarmonyPostfix]
    public static void Postfix(PlayerControl player)
    {
        if (AmongUsClient.Instance.IsGameStarted && GameData.Instance.GetPlayerById(player.PlayerId) is { } p)
        {
            PlayerControlEvents.OnPlayerDisconnect?.Invoke(p);
        }
    }
}