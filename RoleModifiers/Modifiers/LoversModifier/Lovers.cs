using System.Text.RegularExpressions;
using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Framework.Utilities;
using Hazel;
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.GameOptions;
using Lifeboat.RoleModifiers.Modifiers.LoversModifier.Buttons;
using Lifeboat.RoleModifiers.Modifiers.LoversModifier.MonoBehaviours;
using Lifeboat.Roles;
using Lifeboat.WinScreen;
using TMPro;
using UnityEngine;

namespace Lifeboat.RoleModifiers.Modifiers.LoversModifier;

[OptionHeader(nameof(English.Lifeboat_Lovers))]
public sealed class Lovers : BaseModifier
{
    private static GameObject m_TextBubblePrefab;
    public static GameObject TextBubblePrefab
    {
        get
        {
            if (m_TextBubblePrefab.IsThere()) return m_TextBubblePrefab;
                
            m_TextBubblePrefab = ResourceManager.GetAssetBundle("TextBubble").LoadAsset<GameObject>("TextBubble.prefab");
            m_TextBubblePrefab.DontDestroy();
            return m_TextBubblePrefab;
        }
    }
        
    [OptionHeader(nameof(English.Lifeboat_GameOptions_RoleModifiers), int.MinValue)]
    [NumberOption(nameof(English.Lifeboat_Lovers), "Lovers")] 
    public static float LoversAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 0),
        HeaderColor = new Color32(255, 105, 180, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => GeneralOptions.EnableModifiers && LoversAmount > 0 && GameData.Instance.PlayerCount > 1 || GeneralOptions.ShouldShowMeaningless,
    };

    [StringOption(nameof(English.Lifeboat_Lovers_GameOptions_ChaoticLovers), "Lovers_ChaoticLoversChance", 10,
        nameof(English.Lifeboat_Lovers_GameOptions_ChaoticLovers_Random), nameof(English.Lifeboat_Lovers_GameOptions_ChaoticLovers_Likely),
        nameof(English.Lifeboat_Lovers_GameOptions_ChaoticLovers_Guarantee))]
    public static int ChaoticLoversChance = 1;
        
    public byte OtherPlayer { get; }
    public ChatController Chat { get; set; }
        
    public Lovers(byte otherPlayer)
    {
        OtherPlayer = otherPlayer;
    }

    public override void Start()
    {
        Owner.GetRoleManager().NameOverrides.Add(new NameOverride(LoversNameOverride, 15));
            
        if (Owner.AmOwner)
        {
            GameObject newChat = GameObject.Instantiate(HudManager.Instance.Chat.gameObject, HudManager.Instance.transform, true);
            newChat.name = "LoversChatController";
                
            Chat = newChat.GetComponent<ChatController>();
            Chat.SetVisible(false);
            Chat.chatBubPool.ReclaimAll();

            SpriteRenderer chatButton = newChat.transform.Find("ChatButton").GetComponent<SpriteRenderer>();
            chatButton.color = Settings.HeaderColor;
                
            new LoversTextButton(Chat);
        }
    }
        
    public override string GetImportantTaskText()
    {
        return string.Format(LanguageProvider.Current.Lifeboat_Lovers_TaskText, Settings.HeaderColor.ToRGBAString(), 
            GameData.Instance.GetPlayerById(OtherPlayer).PlayerName);
    }

    public override string GetGameSummaryDescription()
    {
        return $"<color=#{Settings.HeaderColor.ToRGBAString()}>[{LanguageProvider.Current.Lifeboat_Lovers_UI_LoverSingular}]</color>";
    }

    public string LoversNameOverride(PlayerControl player, string currentName, bool inMeeting)
    {
        if (!player.IsThere()) return currentName;
        if (player.PlayerId != PlayerControl.LocalPlayer.PlayerId && OtherPlayer != PlayerControl.LocalPlayer.PlayerId && !RoleManager.SeesRolesAsGhost()) 
            return currentName;

        return $"{currentName}{(currentName.Contains("]") ? "" : " ")}<color=#{Settings.HeaderColor.ToRGBAString()}>[♥]</color>";
    }
        
    public void LoversWin()
    {
        TempWinData winData = new()
        {
            SubtitleStringID = nameof(English.Lifeboat_WinReason_Lovers),
            Args = new[] {Owner.Data.PlayerName, GameData.Instance.GetPlayerById(OtherPlayer).PlayerName, Settings.HeaderColor.ToRGBAString()},
            ShowNames = true,
            WinnerBackgroundBarColor = Settings.HeaderColor,
            LoserBackgroundBarColor = Settings.HeaderColor,
            AudioStinger = TempWinData.Stinger.Crewmate,
            WinnerIds = new[] {Owner.PlayerId, OtherPlayer}
        };

        WinScreenNetworking.RpcCustomEndGame(winData);
    }
        
    public static void SetLovers(byte player1, byte player2)
    {
        if (GameData.Instance.GetPlayerById(player1).Object!?.GetRoleManager() is { } manager1 &&
            GameData.Instance.GetPlayerById(player2).Object!?.GetRoleManager() is { } manager2)
        {
            (manager1.MyModifier = new Lovers(player2)).Start();
            (manager2.MyModifier = new Lovers(player1)).Start();
        }
    }

    public static void RpcSendFreeChat(string chatText)
    {
        chatText = Regex.Replace(chatText, "<.*?>", string.Empty);
        if (string.IsNullOrWhiteSpace(chatText)) return;
            
        if (AmongUsClient.Instance.AmClient && HudManager.Instance.IsThere()) AddLoversChat(PlayerControl.LocalPlayer, chatText);
            
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Lovers_SendFreeChat);
        messageWriter.Write(chatText);
        messageWriter.EndMessage();
    }
        
    public static void RpcSendQuickChat(string chatText, QuickChatNetData chatData)
    {
        if (string.IsNullOrWhiteSpace(chatText) || chatData == null) return;
            
        if (AmongUsClient.Instance.AmClient && HudManager.Instance.IsThere()) AddLoversChat(PlayerControl.LocalPlayer, chatText);
            
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Lovers_SendQuickChat);
        chatData.Serialize(messageWriter);
        messageWriter.EndMessage();
    }

    public static void AddLoversChat(PlayerControl sender, string text)
    {
        if (HudManager.Instance.transform.Find("LoversChatController")!?.GetComponent<ChatController>() is not { } chat) return;
        chat.AddChat(sender, "[LOVERSMESSAGE]" + text);

        if (!sender.AmOwner)
        {
            GameObject bubbleObj = GameObject.Instantiate(TextBubblePrefab, HudManager.Instance.transform, true);
            bubbleObj.layer = 5;
            bubbleObj.transform.GetChild(0).gameObject.layer = 5;
            bubbleObj.transform.localScale = new Vector3(0.0666f, 0.0666f, 1);

            TextMessage message = bubbleObj.AddComponent<TextMessage>();
            message.Renderer = bubbleObj.GetComponent<SpriteRenderer>();
            message.Tail = true;
            message.PlainSprite = bubbleObj.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite;
            message.TailSprite = message.Renderer.sprite;
            message.TextMeshPro = bubbleObj.GetComponentInChildren<TextMeshPro>();
            message.Message = text;

            message.Renderer.flipX = true;
            Color color = message.Renderer.color = Palette.PlayerColors[sender.Data.ColorId];
            message.TextMeshPro.color = 1.51f - color.r - color.g - color.b > 0 ? Color.white : Color.black;
            message.UpdateText();

            AspectPosition aspectPosition = bubbleObj.AddComponent<AspectPosition>();
            aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
            aspectPosition.DistanceFromEdge = new Vector3(1.1f + 1.45f, 0.19625f + 0.075f * message.Lines, -10f);
            aspectPosition.AdjustPosition();

            message.Show();
        }
    }
}