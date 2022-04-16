using HarmonyLib;
using Lifeboat.Extensions;

namespace Lifeboat.Patches;

[HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
public static class Console_CanUse_Patch
{
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    public static void Prefix([HarmonyArgument(0)] GameData.PlayerInfo pc, out bool __state)
    {
        __state = pc.IsImpostor;
        if (!pc.Disconnected) pc.IsImpostor = !pc.Object.GetRoleManager().MyRole.CanUseCrewmateConsoles();
    }

    [HarmonyPostfix]
    public static void Postfix([HarmonyArgument(0)] GameData.PlayerInfo pc, bool __state)
    {
        pc.IsImpostor = __state;
    }
}