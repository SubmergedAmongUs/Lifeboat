using HarmonyLib;

namespace Lifeboat.Patches;

[HarmonyPatch(typeof(ImportantTextTask), nameof(ImportantTextTask.AppendTaskText))]
public static class ImportantTextTask_AppendTaskText_Patch
{
    [HarmonyPrefix]
    public static bool Prefix() => false;
}