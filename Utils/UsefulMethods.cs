using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Extensions;
using Framework.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Lifeboat.Utils;

public static class UsefulMethods
{
    public static PlayerControl GetClosestPlayer(float maxDist, bool targetVenting, params byte[] ignoredPlayerIDs)
    {
        IEnumerable<PlayerControl> validPlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => !p.Data.Disconnected && !p.AmOwner && (!p.inVent || targetVenting) && !p.Data.IsDead)
            .Where(p => !ignoredPlayerIDs.Contains(p.PlayerId));

        float maxDistance = maxDist;
        Vector2 myPos = PlayerControl.LocalPlayer.GetTruePosition();

        PlayerControl result = null;
        foreach (PlayerControl player in validPlayers)
        {
            Vector2 deltaVector2 = player.GetTruePosition() - myPos;
            float magnitude = deltaVector2.magnitude;
            if (magnitude <= maxDistance && !PhysicsHelpers.AnyNonTriggersBetween(myPos, deltaVector2.normalized, magnitude, Constants.ShipAndObjectsMask))
            {
                result = player;
                maxDistance = magnitude;
            }
        }

        return result;
    }

    public static void ShowTextToast(string text, float delay = 1.25f)
    {
        HudManager.Instance.StartCoroutine(CoTextToast(text, delay));
    }

    private static IEnumerator CoTextToast(string text, float delay)
    {
        GameObject taskOverlay = Object.Instantiate(HudManager.Instance.TaskCompleteOverlay.gameObject, HudManager.Instance.transform);
        taskOverlay.SetActive(true);
        taskOverlay.GetComponentInChildren<TextTranslatorTMP>().DestroyImmediate();
        taskOverlay.GetComponentInChildren<TextMeshPro>().text = text;
            
        yield return Effects.Slide2D(taskOverlay.transform, new Vector2(0f, -8f), Vector2.zero, 0.25f);
            
        for (float time = 0f; time < delay; time += Time.deltaTime)
        {
            yield return null;
        }
            
        yield return Effects.Slide2D(taskOverlay.transform, Vector2.zero, new Vector2(0f, 8f), 0.25f);
            
        taskOverlay.SetActive(true);
        taskOverlay.Destroy();
    }

    public static IEnumerator CoFlashScreen(Color color)
    {
        SpriteRenderer overlay = GameObject.Instantiate(HudManager.Instance.FullScreen.gameObject, HudManager.Instance.transform).GetComponent<SpriteRenderer>();
        overlay.transform.SetZPos(overlay.transform.position.z - 1);
        overlay.gameObject.SetActive(true);
        overlay.enabled = true;
        overlay.color = color;

        SoundManager.Instance.PlaySound(MapLoader.Skeld.SabotageSound, false, 1f);
        yield return new WaitForSeconds(1f);
            
        overlay.gameObject.Destroy();
    }
}