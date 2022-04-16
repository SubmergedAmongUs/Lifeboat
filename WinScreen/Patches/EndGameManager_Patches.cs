using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Extensions;
using Framework.Localization;
using HarmonyLib;
using Lifeboat.WinScreen.MonoBehaviours;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Lifeboat.WinScreen.Patches;

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
public static class EndGameManager_Start_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(EndGameManager __instance)
    {
        GameObject textObject = Object.Instantiate(__instance.WinText.gameObject, __instance.WinText.transform);
        TextMeshPro text = textObject.GetComponent<TextMeshPro>();
        text.fontSizeMax = 2;
        text.fontSize = 2;
        // ReSharper disable once CoVariantArrayConversion
        text.text = $"<color=#FFFFFF>{string.Format(LanguageProvider.GetLocalizedString(TempWinData.Current.SubtitleStringID), TempWinData.Current.Args)}</color>";
        textObject.transform.localPosition = new Vector3(0, -0.4f, 0);

        SetEverythingUp(__instance);
        __instance.StartCoroutine(CoBegin(__instance, text));
        __instance.Invoke(nameof(EndGameManager.ShowButtons), 1.1f);
        ConsoleJoystick.SetMode_Menu();
        return false;
    }

    public static void SetEverythingUp(EndGameManager __instance)
    {
        List<WinningPlayerData> winners = TempWinData.Current.Winners;

        __instance.WinText.text = TranslationController.Instance.GetString(TempWinData.Current.AmWinner ? StringNames.Victory : StringNames.Defeat);
        if (!TempWinData.Current.AmWinner) __instance.WinText.color = Color.red;
            
        __instance.BackgroundBar.material.SetColor("_Color", TempWinData.Current.AmWinner 
            ? TempWinData.Current.WinnerBackgroundBarColor 
            : TempWinData.Current.LoserBackgroundBarColor);

        switch (TempWinData.Current.AudioStinger)
        {
            case TempWinData.Stinger.Crewmate:
                SoundManager.Instance.PlayDynamicSound("Stinger", __instance.CrewStinger, false, (Action<AudioSource, float>) __instance.GetStingerVol);
                break;
                
            case TempWinData.Stinger.Impostor:
                SoundManager.Instance.PlayDynamicSound("Stinger", __instance.ImpostorStinger, false, (Action<AudioSource, float>) __instance.GetStingerVol);
                break;
                
            case TempWinData.Stinger.Disconnect:
                SoundManager.Instance.PlaySound(__instance.DisconnectStinger, false, 1f);
                break;
        }

        int num = Mathf.CeilToInt(7.5f);
        List<WinningPlayerData> list = winners.OrderBy(b => !b.IsYou ? 0 : -1).ToList();
        for (int i = 0; i < list.Count; i++)
        {
            WinningPlayerData winningPlayerData2 = list[i];
            int num2 = i % 2 == 0 ? -1 : 1;
            int num3 = (i + 1) / 2;
            float num4 = num3 / (float) num;
            float num5 = Mathf.Lerp(1f, 0.75f, num4);
            float num6 = i == 0 ? -8 : -1;
            PoolablePlayer poolablePlayer = Object.Instantiate(__instance.PlayerPrefab, __instance.transform);
            poolablePlayer.transform.localPosition = new Vector3(1f * num2 * num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + num3 * 0.01f) * 0.9f;
            float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
            Vector3 vector = new(num7, num7, 1f);
            poolablePlayer.transform.localScale = vector;
            if (winningPlayerData2.IsDead)
            {
                poolablePlayer.Body.sprite = __instance.GhostSprite;
                poolablePlayer.SetDeadFlipX(i % 2 == 0);
            }
            else
            {
                poolablePlayer.SetFlipX(i % 2 == 0);
            }
                
            if (!winningPlayerData2.IsDead) HatManager.Instance.SetSkin(poolablePlayer.Skin.layer, winningPlayerData2.SkinId);
            else poolablePlayer.HatSlot.color = new Color(1f, 1f, 1f, 0.5f);

            PlayerControl.SetPlayerMaterialColors(winningPlayerData2.ColorId, poolablePlayer.Body);
            poolablePlayer.HatSlot.SetHat(winningPlayerData2.HatId, winningPlayerData2.ColorId);
            PlayerControl.SetPetImage(winningPlayerData2.PetId, winningPlayerData2.ColorId, poolablePlayer.PetSlot);
            poolablePlayer.NameText.gameObject.SetActive(TempWinData.Current.ShowNames);
                
            poolablePlayer.NameText.text = winningPlayerData2.Name;
            if (winningPlayerData2.IsImpostor) poolablePlayer.NameText.color = Palette.ImpostorRed;
            poolablePlayer.NameText.transform.localScale = vector.Inverse();
            poolablePlayer.NameText.transform.SetLocalZ(-15f);
        }
    }

    public static IEnumerator CoBegin(EndGameManager __instance, TextMeshPro otherText)
    {
        Color c = __instance.WinText.color;
        Color fade = Color.black;
        Vector3 titlePos = __instance.WinText.transform.localPosition;
        float timer = 0f;

        MakeSummaryText(__instance);
        while (timer < 3f)
        {
            timer += Time.deltaTime;
            float num = Mathf.Min(1f, timer / 3f);
            __instance.Foreground.material.SetFloat("_Rad", __instance.ForegroundRadius.ExpOutLerp(num * 2f));
            fade.a = Mathf.Lerp(1f, 0f, num * 3f);
            __instance.FrontMost.color = fade;
            c.a = Mathf.Clamp(FloatRange.ExpOutLerp(num, 0f, 1f), 0f, 1f);
            __instance.WinText.color = c;
            otherText.color = c;
            titlePos.y = 2.7f - num * 0.35f;
            __instance.WinText.transform.localPosition = titlePos;
            yield return null;
        }

        __instance.FrontMost.gameObject.SetActive(false);
    }
        
    public static void MakeSummaryText(EndGameManager __instance)
    {
        GameObject summaryTextObj = Object.Instantiate(__instance.WinText.transform.GetChild(0).gameObject);

        summaryTextObj.AddComponent<SummaryText>();
    }
}