using HarmonyLib;
using Lifeboat.Enums;
using Lifeboat.Extensions;

namespace Lifeboat.Patches;

[HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
public static class GameData_RecomputeTaskCounts_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(GameData __instance)
    {
        __instance.TotalTasks = 0;
        __instance.CompletedTasks = 0;
        for (int i = 0; i < __instance.AllPlayers.Count; i++)
        {
            GameData.PlayerInfo playerInfo = __instance.AllPlayers.ToArray()[i];
            if (!playerInfo.Disconnected && playerInfo.Tasks != null && playerInfo.Object && (PlayerControl.GameOptions.GhostsDoTasks || !playerInfo.IsDead) && !playerInfo.IsImpostor)
            {
                if (playerInfo.Object && playerInfo.Object.GetRoleManager().MyRole is {Alignment: Alignment.Crewmate})
                {
                    for (int j = 0; j < playerInfo.Tasks.Count; j++)
                    {
                        __instance.TotalTasks++;
                        if (playerInfo.Tasks.ToArray()[j].Complete)
                        {
                            __instance.CompletedTasks++;
                        }
                    }
                }
            }
        }
            
        return false;
    }
}