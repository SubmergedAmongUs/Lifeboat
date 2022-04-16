namespace Lifeboat.Events;

public static class PlayerControlEvents
{
    public static void Clear()
    {
        OnPlayerMurder = null;
        OnPlayerCustomMurder = null;
        OnPlayerExile = null;
        OnPlayerRevive = null;
        OnPlayerDisconnect = null;
    }
    
    public delegate void PlayerMurderEvent(PlayerControl killer, PlayerControl target);
    public static PlayerMurderEvent OnPlayerMurder;

    public delegate void PlayerCustomMurderEvent(PlayerControl killer, PlayerControl target);
    public static PlayerCustomMurderEvent OnPlayerCustomMurder;
        
    public delegate void PlayerExileEvent(PlayerControl player);
    public static PlayerExileEvent OnPlayerExile;

    public delegate void PlayerReviveEvent(PlayerControl player);
    public static PlayerReviveEvent OnPlayerRevive;
        
    public delegate void PlayerDisconnectEvent(GameData.PlayerInfo player);
    public static PlayerDisconnectEvent OnPlayerDisconnect;
}