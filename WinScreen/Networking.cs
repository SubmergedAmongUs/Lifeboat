using System.Linq;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;

namespace Lifeboat.WinScreen;

public static class WinScreenNetworking
{
    public delegate void d_ModifyWinData(TempWinData data);

    public static d_ModifyWinData ModifyWinData;
        
    public static void RpcCustomEndGame(TempWinData data)
    {
        ModifyWinData?.Invoke(data);
        data.WinnerIds = data.WinnerIds.Distinct().ToArray();
            
        if (AmongUsClient.Instance.AmHost)
        {
            WriteCustomEndGame(data);
        }
        else
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.General_EndGame, SendOption.None, -1);
            messageWriter.Write(data.SubtitleStringID);
            messageWriter.Write(data.Args.Length);
            foreach (string t in data.Args) messageWriter.Write(t);
            messageWriter.Write(data.ShowNames);
            messageWriter.Write(data.WinnerBackgroundBarColor);
            messageWriter.Write(data.LoserBackgroundBarColor);
            messageWriter.Write((byte) data.AudioStinger);
            messageWriter.WriteBytesAndSize(data.WinnerIds);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
    }

    public static void HandleCustomEndGame(MessageReader reader)
    {
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

        WriteCustomEndGame(winData);
    }

    public static void WriteCustomEndGame(TempWinData data)
    {
        MessageWriter messageWriter = AmongUsClient.Instance.StartEndGame();
            
        messageWriter.Write((byte) 0x69);
        messageWriter.Write(false);
            
        messageWriter.Write(data.SubtitleStringID);
        messageWriter.Write(data.Args.Length);
        foreach (string t in data.Args) messageWriter.Write(t);
        messageWriter.Write(data.ShowNames);
        messageWriter.Write(data.WinnerBackgroundBarColor);
        messageWriter.Write(data.LoserBackgroundBarColor);
        messageWriter.Write((byte) data.AudioStinger);
        messageWriter.WriteBytesAndSize(data.WinnerIds);

        AmongUsClient.Instance.FinishEndGame(messageWriter);
    }
}