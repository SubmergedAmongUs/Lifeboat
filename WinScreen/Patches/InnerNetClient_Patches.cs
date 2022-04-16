using System.Linq;
using HarmonyLib;
using Hazel;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using Lifeboat.Extensions;
using Lifeboat.Roles;

namespace Lifeboat.WinScreen.Patches;

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
public static class InnerNetClient_HandleMessage_Patches
{
    [HarmonyPrefix]
    public static bool Prefix(InnerNetClient __instance, [HarmonyArgument(0)] MessageReader reader, [HarmonyArgument(1)] SendOption sendOption)
    {
        if (reader.Tag != Tags.EndGame) return true;
        int readHead = reader.readHead;
        int position = reader._position;

        reader.ReadInt32();
        byte gameOverReason = reader.ReadByte();
            
        reader.readHead = readHead;
        reader._position = position;

        if (gameOverReason != 0x69) return true;

        int num3 = reader.ReadInt32();
            
        if (__instance.GameId == num3)
        {
            __instance.GameState = (InnerNetClient.GameStates) GameStates.Ended;
            List<ClientData> obj2 = __instance.allClients;
            lock (obj2)
            {
                __instance.allClients.Clear();
            }
            GameOverReason reason = (GameOverReason) reader.ReadByte();
            bool showAd = reader.ReadBoolean();
                
            TempWinData winData = new()
            {
                SubtitleStringID = reader.ReadString()
            };

            int argAmount = reader.ReadInt32();
            winData.Args = new string[argAmount];
            for (int i = 0; i < argAmount; i++) winData.Args[i] = reader.ReadString();
                
            winData.ShowNames = reader.ReadBoolean();
            winData.WinnerBackgroundBarColor = reader.ReadColor32();
            winData.LoserBackgroundBarColor = reader.ReadColor32();
            winData.AudioStinger = (TempWinData.Stinger) reader.ReadByte();
            winData.WinnerIds = reader.ReadBytesAndSize();
            winData.Winners = new System.Collections.Generic.List<WinningPlayerData>();

            foreach (byte playerId in winData.WinnerIds)
            {
                winData.Winners.Add(new WinningPlayerData(GameData.Instance.GetPlayerById(playerId)));
            }

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                BaseRole playerRole = player.GetRoleManager().MyRole;
                bool hasWon = winData.WinnerIds.Contains(player.PlayerId);
                string text = playerRole.GetGameSummaryDescription(hasWon);
                winData.RoleData.Add((player.PlayerId, text));
            }
                
            winData.AmWinner = winData.WinnerIds.Contains(PlayerControl.LocalPlayer.PlayerId);
            TempWinData.Current = winData;

            List<Action> obj = __instance.Dispatcher;
            lock (obj)
            {
                __instance.Dispatcher.Add((System.Action) (() => __instance.OnGameEnd(reason, showAd)));
            }
        }
            
        return false;
    }
}