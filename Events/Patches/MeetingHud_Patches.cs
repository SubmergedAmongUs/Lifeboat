using HarmonyLib;

namespace Lifeboat.Events.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingHud_Start_Patch
{
    [HarmonyPostfix]
    public static void Postfix(MeetingHud __instance)
    {
        MeetingHudEvents.OnMeetingStart?.Invoke(__instance);
    }
}
    
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingHud_Update_Patch
{
    [HarmonyPostfix]
    public static void Postfix(MeetingHud __instance)
    {
        MeetingHudEvents.OnMeetingUpdate?.Invoke(__instance);
    }
}