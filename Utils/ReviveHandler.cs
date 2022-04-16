using Lifeboat.Extensions;

namespace Lifeboat.Utils;

public static class ReviveHandler
{
    public static void RevivePlayer(PlayerControl player)
    {
        player.Revive();
    }

    public static void ReviveAndRemoveBody(PlayerControl player)
    {
        DeadBodyExtensions.DestroyBody(player.PlayerId);
        RevivePlayer(player);
    }
}