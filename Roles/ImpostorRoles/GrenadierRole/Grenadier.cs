using System.Collections;
using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Lifeboat.Attributes;
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.Roles.ImpostorRoles.GrenadierRole.Buttons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Lifeboat.Roles.ImpostorRoles.GrenadierRole;

[OptionHeader(nameof(English.Lifeboat_Grenadier))]
public sealed class Grenadier : Impostor
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_ImpostorRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Grenadier), "Grenadier")] 
    public static float GrenadierAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 57),
        HeaderColor = new Color32(143, 143, 143, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(GrenadierAmount, Alignment.Impostor),
    };
        
    [ToggleOption(nameof(English.Lifeboat_Grenadier_GameOptions_SmokeThroughWalls), "Can Flash Through Walls", 10)] 
    public static bool SmokeThroughWalls;

    [NumberOption(nameof(English.Lifeboat_Grenadier_GameOptions_Duration), "Flashbang Duration", 9,
        3, 10, 1, false, "{0}s")] 
    public static float SmokeDuration = 5;
        
    [NumberOption(nameof(English.Lifeboat_Grenadier_GameOptions_Cooldown), "Flashbang Cooldown", 8,
        5, 60, 2.5f, false, "{0:0.0}s")] 
    public static float SmokeCooldown = 25;
        
    #endregion

    public override string RoleStringID => nameof(English.Lifeboat_Grenadier);
    public override Color32 Color => Settings.HeaderColor;

    public override void Start()
    {
        base.Start();
        if (Owner.AmOwner) new SmokeButton(this);
    }
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Grenadier_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public void Smoke(float alpha = 1f)
    {
        PlayerControl.LocalPlayer.GetRoleManager().StartCoroutine(CoSmokeScreen(alpha));
    }

    public IEnumerator CoSmokeScreen(float alpha = 1f)
    {
        GameObject fullScreenObj = GameObject.Instantiate(HudManager.Instance.FullScreen.gameObject, HudManager.Instance.transform);
        SpriteRenderer fullScreen = fullScreenObj.GetComponent<SpriteRenderer>();

        fullScreen.enabled = true;
        fullScreen.gameObject.SetActive(true);
        fullScreen.color = UnityEngine.Color.clear;

        Color newColor = new(0.5f, 0.5f, 0.5f, alpha);
        const float initialLerpTime = 0.15f;
        const float finalLerpTime = 0.5f;
            
        for (float t = 0; t < initialLerpTime; t += Time.deltaTime)
        {
            if (MeetingHud.Instance)
            {
                Object.Destroy(fullScreenObj);
                yield break;
            }
                
            fullScreen.color = UnityEngine.Color.Lerp(UnityEngine.Color.clear, newColor, t / initialLerpTime);
            yield return null;
        }

        fullScreen.color = newColor;

        for (float t = 0; t < SmokeDuration- initialLerpTime - finalLerpTime; t += Time.deltaTime)
        {
            if (MeetingHud.Instance)
            {
                Object.Destroy(fullScreenObj);
                yield break;
            }
                
            yield return null;
        }
            
        for (float t = 0; t < finalLerpTime; t += Time.deltaTime)
        {
            if (MeetingHud.Instance)
            {
                Object.Destroy(fullScreenObj);
                yield break;
            }
                
            fullScreen.color = UnityEngine.Color.Lerp(newColor, UnityEngine.Color.clear, t / finalLerpTime);
            yield return null;
        }
            
        Object.Destroy(fullScreenObj);
    }

    public IEnumerator CoDoNameOverride(PlayerControl player)
    {
        NameOverride nameOverride = new(GrenadierNameOverride);
        player.GetRoleManager().NameOverrides.Add(nameOverride);
        yield return new WaitForSeconds(SmokeDuration - 0.65f);
        nameOverride.Dispose();
    }
        
    public string GrenadierNameOverride(PlayerControl player, string currentName, bool inMeeting)
    {
        if (!player.IsThere() || inMeeting) return currentName;
        if (!PlayerControl.LocalPlayer.Data.IsImpostor && !RoleManager.SeesRolesAsGhost()) return currentName;

        return $"{currentName}{(currentName.Contains("]") ? "" : " ")}<color=#{Color.ToRGBAString()}>[※]</color>";
    }
}