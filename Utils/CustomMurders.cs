using Framework.Extensions;
using UnityEngine;

namespace Lifeboat.Utils;

public static class CustomMurders
{
    public static void MurderNoAnim(PlayerControl murderer, PlayerControl target)
    {
        if (AmongUsClient.Instance.IsGameOver)
        {
            return;
        }

        GameData.PlayerInfo data = target.Data;
        if (data == null || data.IsDead)
        {
            return;
        }
            
        target.gameObject.layer = LayerMask.NameToLayer("Ghost");
        if (target.AmOwner)
        {
            StatsManager instance2 = StatsManager.Instance;
            uint num2 = instance2.TimesMurdered;
            instance2.TimesMurdered = num2 + 1U;
            if (Minigame.Instance)
            {
                try
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }
                catch
                {
                    // Ignore
                }
            }

            HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
            target.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
            target.RpcSetScanner(false);
        }

        murderer.MyPhysics.StartCoroutine(murderer.KillAnimations.Random().CoPerformKill(murderer, target));
    }
}