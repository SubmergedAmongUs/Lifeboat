using Framework.Extensions;
using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace Lifeboat.RoleModifiers.Modifiers.LoversModifier.Patches;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class ChatController_AddChat_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] PlayerControl sourcePlayer, [HarmonyArgument(1)] ref string chatText)
    {
        if (__instance.name != "LoversChatController") return true;
            
        if (!chatText.StartsWith("[LOVERSMESSAGE]"))
        {
            HudManager.Instance.transform.Find("ChatUi").GetComponent<ChatController>().AddChat(sourcePlayer, chatText);
            return false;
        }

        chatText = chatText[15..];
            
        return true;
    }
}
    
[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChatNote))]
public static class ChatController_AddChatNote_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] GameData.PlayerInfo srcPlayer, [HarmonyArgument(1)] ChatNoteTypes noteTypes)
    {
        if (__instance.name != "LoversChatController") return true;
            
        HudManager.Instance.transform.Find("ChatUi").GetComponent<ChatController>().AddChatNote(srcPlayer, noteTypes);
        return false;
    }
}
    
[HarmonyPatch(typeof(ChatController._CoClose_d__34), nameof(ChatController._CoClose_d__34.MoveNext))]
public static class ChatController_CoClose_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ChatController._CoClose_d__34 __instance, bool __result)
    {
        if (__result || __instance._timer_5__4 < 0.15f || __instance.__4__this.name != "LoversChatController") return;
            
        __instance.__4__this.gameObject.SetActive(false);
        HudManager.Instance.Chat = HudManager.Instance.transform.Find("ChatUi").GetComponent<ChatController>();
        HudManager.Instance.Chat.gameObject.SetActive(PlayerControl.LocalPlayer.Data.IsDead || MeetingHud.Instance.IsThere());
    }
}
    
[HarmonyPatch(typeof(ChatController), nameof(ChatController.ForceClosed))]
public static class ChatController_ForceClosed_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ChatController __instance)
    {
        if (__instance.name != "LoversChatController") return;
            
        __instance.gameObject.SetActive(false);
        HudManager.Instance.Chat = HudManager.Instance.transform.Find("ChatUi").GetComponent<ChatController>();
        HudManager.Instance.Chat.gameObject.SetActive(PlayerControl.LocalPlayer.Data.IsDead || MeetingHud.Instance.IsThere());
    }
}
        
[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatController_SendChat_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(ChatController __instance)
    {
        if (__instance.name != "LoversChatController") return true;

        float num = 3f - __instance.TimeSinceLastMessage;
        if (num > 0f)
        {
            __instance.SendRateMessage.gameObject.SetActive(true);
            __instance.SendRateMessage.text = TranslationController.Instance.GetString(StringNames.ChatRateLimit, Mathf.CeilToInt(num));
            return false;
        }
        if (__instance.quickChatData.qcType != QuickChatNetType.None)
        {
            Lovers.RpcSendQuickChat(__instance.TextArea.text, __instance.quickChatData);
        }
        else if (SaveManager.ChatModeType == QuickChatModes.FreeChatOrQuickChat)
        {
            Lovers.RpcSendFreeChat(__instance.TextArea.text);
        }
        __instance.TimeSinceLastMessage = 0f;
        __instance.TextArea.Clear();
        __instance.quickChatMenu.ResetGlyphs();
        __instance.quickChatData.qcType = QuickChatNetType.None;

        return false;
    }
}
    
[HarmonyPatch(typeof(ChatController), nameof(ChatController.SetPosition))]
public static class ChatController_SetPosition_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(ChatController __instance, [HarmonyArgument(0)] MeetingHud meeting)
    {
        if (__instance.name != "LoversChatController") return true;
            
        __instance.ForceClosed();
        HudManager.Instance.Chat.SetPosition(meeting);
        return false;
    }
}