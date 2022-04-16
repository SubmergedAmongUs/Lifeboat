using System.Collections.Generic;
using UnityEngine;

namespace Lifeboat.WinScreen;

public sealed class TempWinData
{
    public static TempWinData Current;
        
    public enum Stinger : byte
    {
        Crewmate,
        Impostor,
        Disconnect
    }
        
    public string SubtitleStringID;
    public string[] Args;
    public bool ShowNames = true;
    public bool AmWinner;
        
    public Color32 WinnerBackgroundBarColor = Color.yellow;
    public Color32 LoserBackgroundBarColor = Color.yellow;
    public Stinger AudioStinger = Stinger.Crewmate;
        
    public byte[] WinnerIds = {};
    public List<WinningPlayerData> Winners = new();
    public List<(byte playerId, string name)> RoleData = new();
}