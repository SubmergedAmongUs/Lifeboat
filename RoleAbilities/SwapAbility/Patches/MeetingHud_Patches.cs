using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Extensions;
using HarmonyLib;
using Lifeboat.Extensions;
using Lifeboat.RoleAbilities.SwapAbility.MonoBehaviours;
using UnityEngine;

namespace Lifeboat.RoleAbilities.SwapAbility.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingHud_Start_Patch
{
    [HarmonyPrefix]
    public static void Prefix(MeetingHud __instance)
    {
        if (!PlayerControl.LocalPlayer.Data.IsDead && 
            PlayerControl.LocalPlayer.GetRoleManager().MyRole.Abilities.FirstOrDefault(a => a is SwapAbility) is SwapAbility swapper)
        {
            swapper.Selected.Clear();
                
            SwapperMeetingBehaviour behaviour = __instance.gameObject.AddComponent<SwapperMeetingBehaviour>();
            behaviour.Meeting = __instance;
            behaviour.Swapper = swapper;
        }
    }
}
    
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
public static class MeetingHud_Close_Patch
{
    [HarmonyPrefix, HarmonyPriority(Priority.Last)]
    public static bool Prefix(MeetingHud __instance)
    {
        List<SwapAbility> swappers = PlayerControl.AllPlayerControls.ToArray()
            .Select(p => p.GetRoleManager().MyRole)
            .SelectMany(r => r.Abilities)
            .OfType<SwapAbility>()
            .Where(s => s.Owner.Owner && !s.Owner.Owner.Data.IsDead && s.SwapOne != byte.MaxValue && s.SwapTwo != byte.MaxValue)
            .ToList();
            
        __instance.StartCoroutine(CoSwapperClose(__instance, swappers));
            
        return false;
    }

    public static IEnumerator CoSwapperClose(MeetingHud __instance, List<SwapAbility> swappers)
    {
        if (swappers.Count > 0)
        {
            swappers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                
            const float duration = 0.5f;
            for (float t = 0; t <= duration; t += Time.deltaTime)
            {
                foreach (SwapAbility swapper in swappers)
                {
                    try
                    {
                        PlayerVoteArea first = __instance.playerStates.FirstOrDefault(p => swapper.SwapOne == p.TargetPlayerId);
                        PlayerVoteArea second = __instance.playerStates.FirstOrDefault(p => swapper.SwapTwo == p.TargetPlayerId);

                        if (first && second)
                        {
                            float alpha = 1 - t / duration;
                            first.NameText.alpha = alpha;
                            second.NameText.alpha = alpha;

                            Color alphaColor = new(1, 1, 1, alpha);
                            if (first.PlayerIcon)
                            {
                                first.PlayerIcon.Body.color = alphaColor;
                                first.PlayerIcon.HatSlot.color = alphaColor;
                                first.PlayerIcon.Skin.layer.color = alphaColor;
                            }

                            if (second.PlayerIcon)
                            {
                                second.PlayerIcon.Body.color = alphaColor;
                                second.PlayerIcon.HatSlot.color = alphaColor;
                                second.PlayerIcon.Skin.layer.color = alphaColor;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LifeboatPlugin.Log.LogError(e);
                    }
                }

                yield return null;
            }

            foreach (SwapAbility swapper in swappers)
            {
                try
                {
                    PlayerVoteArea first = __instance.playerStates.FirstOrDefault(p => swapper.SwapOne == p.TargetPlayerId);
                    PlayerVoteArea second = __instance.playerStates.FirstOrDefault(p => swapper.SwapTwo == p.TargetPlayerId);

                    if (first && second)
                    {
                        string firstText = first.NameText.text;
                        string secondText = second.NameText.text;

                        first.TargetPlayerId = swapper.SwapTwo;
                        second.TargetPlayerId = swapper.SwapOne;

                        first.NameText.text = secondText;
                        second.NameText.text = firstText;

                        first.SetCosmetics(GameData.Instance.GetPlayerById(swapper.SwapTwo));
                        second.SetCosmetics(GameData.Instance.GetPlayerById(swapper.SwapOne));
                    }
                }
                catch (Exception e)
                {
                    LifeboatPlugin.Log.LogError(e);
                }

                try
                {
                    if (__instance.exiledPlayer != null)
                    {
                        if (__instance.exiledPlayer.PlayerId == swapper.SwapOne)
                        {
                            __instance.exiledPlayer = GameData.Instance.GetPlayerById(swapper.SwapTwo);
                            Submerged.Map.Patches.MeetingHud_Close_Patch.Postfix(__instance);
                        }
                        else if (__instance.exiledPlayer.PlayerId == swapper.SwapTwo)
                        {
                            __instance.exiledPlayer = GameData.Instance.GetPlayerById(swapper.SwapOne);
                            Submerged.Map.Patches.MeetingHud_Close_Patch.Postfix(__instance);
                        }
                    }
                }
                catch (Exception e)
                {
                    LifeboatPlugin.Log.LogError(e);
                }
            }
                
            for (float t = 0; t <= duration; t += Time.deltaTime)
            {
                foreach (SwapAbility swapper in swappers)
                {
                    try
                    {
                        PlayerVoteArea first = __instance.playerStates.FirstOrDefault(p => swapper.SwapOne == p.TargetPlayerId);
                        PlayerVoteArea second = __instance.playerStates.FirstOrDefault(p => swapper.SwapTwo == p.TargetPlayerId);

                        if (first && second)
                        {
                            float alpha = t / duration;
                            first.NameText.alpha = alpha;
                            second.NameText.alpha = alpha;

                            Color alphaColor = new(1, 1, 1, alpha);
                            if (first.PlayerIcon)
                            {
                                first.PlayerIcon.Body.color = alphaColor;
                                first.PlayerIcon.HatSlot.color = alphaColor;
                                first.PlayerIcon.Skin.layer.color = alphaColor;
                            }

                            if (second.PlayerIcon)
                            {
                                second.PlayerIcon.Body.color = alphaColor;
                                second.PlayerIcon.HatSlot.color = alphaColor;
                                second.PlayerIcon.Skin.layer.color = alphaColor;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LifeboatPlugin.Log.LogError(e);
                    }
                }

                yield return null;
            }
                
            try
            {
                foreach (SwapAbility swapper in swappers)
                {
                    swapper.SwapOne = byte.MaxValue;
                    swapper.SwapTwo = byte.MaxValue;
                }
            }
            catch (Exception e)
            {
                LifeboatPlugin.Log.LogError(e);
            }
                
            yield return new WaitForSeconds(2.5f);
        }

        GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
        HudManager.Instance.Chat.SetPosition(null);
        HudManager.Instance.Chat.SetVisible(data.IsDead);
        HudManager.Instance.Chat.BanButton.Hide();
        __instance.StartCoroutine(__instance.CoStartCutscene());
    }
        
    [HarmonyPostfix]
    public static void Postfix(MeetingHud __instance)
    {
        if (!PlayerControl.LocalPlayer.Data.IsDead && 
            PlayerControl.LocalPlayer.GetRoleManager().MyRole.Abilities.FirstOrDefault(a => a is SwapAbility) is SwapAbility swapper)
        {
            swapper.Selected.Clear();
        }
    }
}