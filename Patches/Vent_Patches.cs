using HarmonyLib;
using Lifeboat.Extensions;

namespace Lifeboat.Patches;

[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
public static class Vent_CanUse_Patch
{
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    public static void Prefix([HarmonyArgument(0)] GameData.PlayerInfo pc, out bool __state)
    {
        __state = pc.IsImpostor;
        if (!pc.Disconnected) pc.IsImpostor = pc.Object.GetRoleManager().MyRole.CanUseVents();
    }

    [HarmonyPostfix]
    public static void Postfix([HarmonyArgument(0)] GameData.PlayerInfo pc, bool __state)
    {
        pc.IsImpostor = __state;
    }
}