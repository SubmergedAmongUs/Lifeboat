using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Extensions;
using Lifeboat.Extensions;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.TimeLordRole;

public static class RewindTime
{
    public static Coroutine RewindRoutine;
        
    public static void StartRewind(TimeLord timeLord)
    {
        if (Minigame.Instance) Minigame.Instance.Close();
        RewindRoutine = PlayerControl.LocalPlayer.GetRoleManager().StartCoroutine(CoRewind(timeLord));
    }
        
    public static IEnumerator CoRewind(TimeLord timeLord)
    {
        List<byte> list = new();
        int Complete = 0;

        SpriteRenderer overlay = GameObject.Instantiate(HudManager.Instance.FullScreen.gameObject, HudManager.Instance.transform).GetComponent<SpriteRenderer>();
        overlay.transform.SetZPos(overlay.transform.position.z - 1);
        overlay.gameObject.SetActive(true);
        overlay.enabled = true;
        overlay.color = new Color32(0, 65, 130, 70);

        while (Complete != PlayerControl.AllPlayerControls.Count)
        {
            if (MeetingHud.Instance)
            {
                overlay.color = Color.clear;
            }
                
            for (int i = 0; i < 5; i++)
            {
                foreach ((byte playerId, LinkedList<PlayerPointInTime> value) in timeLord.PointInTimesMap)
                {
                    GameData.PlayerInfo data = GameData.Instance.GetPlayerById(playerId);
                    if (!value.Any() || data == null || data.Disconnected || !data.Object || data.Object.inVent)
                    {
                        if (!list.Contains(playerId)) Complete++;
                        list.Add(playerId);
                        continue;
                    }
                    if (i == 0) value.Last.Value.Rewind(data.Object);
                    value.RemoveLast();
                }

                if (timeLord.MapPointInTimes.Any())
                {
                    if (i == 0) timeLord.MapPointInTimes.Last.Value.Rewind();
                    timeLord.MapPointInTimes.RemoveLast();
                }
            }
                
            yield return null;
        }

        overlay.gameObject.Destroy();
        RewindRoutine = null;

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (!player.inVent)
            {
                player.MyPhysics.ResetMoveState(player.CanMove);
                player.MyPhysics.ResetAnimState();
            }
        }
    }
}