using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Extensions;
using Framework.Localization.Extensions;
using Framework.Utilities;
using Lifeboat.Extensions;
using Lifeboat.Roles;
using Submerged.Minigames.CustomMinigames.CamsSabotage;
using Submerged.Minigames.CustomMinigames.DoorSabotage;
using Submerged.Minigames.CustomMinigames.RetrieveOxygenMask;
using Submerged.Minigames.CustomMinigames.SpawnIn;
using Submerged.Minigames.CustomMinigames.StabilizeWaterLevels;
using Submerged.Minigames.CustomMinigames.Surveillance;
using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Lifeboat.Buttons;

public abstract class BaseButton
{
    // Standard properties
    public virtual float CurrentTime { get; set; } = 5f;
    public virtual float Cooldown { get; set; } = 5f;
    public virtual float EffectDuration { get; set; } = 0f; // Doesnt show if 0
    public virtual bool IsModifierButton { get; } = false;
    public virtual bool CanUseInVents { get; } = false;

    public virtual KeyCode[] Keybinds { get; } = {};

    // Target Properties
    public Coroutine EffectCoroutine { get; set; }

    // Set during instantiation
    public GameObject GameObject { get; }
    public KillButtonManager KillButtonManager { get; }
    public AspectPosition AspectPosition { get; }
    public PassiveButton PassiveButton { get; }
    public Button.ButtonClickedEvent ClickEvent { get; }
        
    public BaseButton()
    {
        // Instantiate new kill button
        GameObject = GameObject.Instantiate(HudManager.Instance.KillButton.gameObject, HudManager.Instance.transform);
        GameObject.name = GetType().Name;

        // Set cooldown
        KillButtonManager = GameObject.GetComponent<KillButtonManager>();
        KillButtonManager.SetCoolDown(CurrentTime, Cooldown);

        // Setup positioning
        AspectPosition = GameObject.GetComponent<AspectPosition>();
        AspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        AspectPosition.DistanceFromEdge = new Vector3(0.7f, 0.7f, -5f);
        AspectPosition.updateAlways = true;

        // Setup click actions
        RoleManager roleManager = PlayerControl.LocalPlayer.GetRoleManager();
        PassiveButton = GameObject.GetComponent<PassiveButton>();
        ClickEvent = new Button.ButtonClickedEvent();
        PassiveButton.OnClick = ClickEvent;
        ClickEvent.RemoveAllListeners();
        ClickEvent.AddListener((System.Action) (() =>
        {
            if (!CanUse() || CurrentTime > 0) return;
            
            CurrentTime = Cooldown;
            if (EffectDuration > 0)
            {
                EffectCoroutine = roleManager.StartCoroutine(ShowEffectDuration(EffectDuration));
            }
                
            OnClick();
        }));
            
        // Add itself to role manager buttons list
        roleManager.Buttons.Add(this);
            
        SetupButtonText();
    }

    public virtual void SetupButtonText()
    {
        GameObject.transform.Find("Text_TMP").gameObject.SetActive(false);
    }
        
    public virtual void OnClick() { }
        
    public void SetSprite(string name, float ppu = 100f)
    {
        ImageTranslator translator = KillButtonManager.GetComponent<ImageTranslator>();
        if (translator) GameObject.DestroyImmediate(translator);
            
        KillButtonManager.renderer.sprite = ResourceManager.GetSprite(name, ppu);
    }

    public void SetSprite(Sprite sprite)
    {
        ImageTranslator translator = KillButtonManager.GetComponent<ImageTranslator>();
        if (translator) GameObject.DestroyImmediate(translator);
            
        KillButtonManager.renderer.sprite = sprite;
    }

    public void SetPosition(AspectPosition.EdgeAlignments alignments = AspectPosition.EdgeAlignments.LeftBottom, bool autoPosition = true, int shift = 0)
    {
        AspectPosition.Alignment = alignments;
        Vector3 distanceFromEdge = new(0.7f, 0.7f, -5f);
        int otherButtons = 0 + shift;
        if (autoPosition)
        {
            // Get the count of other buttons in that spot
            List<KillButtonManager> currentButtons = GameObject.FindObjectsOfType<KillButtonManager>().ToList();
            foreach (KillButtonManager button in currentButtons)
            {
                if (button.GetHashCode() == KillButtonManager.GetHashCode()) continue;
                    
                if (button.gameObject.GetComponent<AspectPosition>().Alignment == alignments)
                {
                    otherButtons++;
                }
            }
        }

        if (alignments == AspectPosition.EdgeAlignments.RightBottom)
        {
            distanceFromEdge.x += 1.45f;
        }

        distanceFromEdge.y += 1.3f * ((otherButtons) % 3);
        distanceFromEdge.x += 1.45f * Mathf.Floor((otherButtons) / 3f);
        AspectPosition.DistanceFromEdge = distanceFromEdge;
        AspectPosition.AdjustPosition();
    }

    public void SetTextTransform(Vector3 localPos = default, Vector3 localScale = default)
    {
        if (localPos == default) localPos = new Vector3(0, -0.3654f, 0);
        if (localScale == default) localScale = new Vector3(0.8f, 0.8f, 1);

        Transform text = GameObject.transform.Find("Text_TMP").transform;
        text.localPosition = localPos;
        text.localScale = localScale;
    }
        
    public void SetTextTranslation(string stringId)
    {
        TextMeshPro text = GameObject.transform.Find("Text_TMP").GetComponent<TextMeshPro>();
        text.GetComponent<TextTranslatorTMP>().DestroyImmediate();
        text.SetLocalizedText(stringId);
    }

    public void SetTextColor(Color color)
    {
        TextMeshPro text = GameObject.transform.Find("Text_TMP").GetComponent<TextMeshPro>();
        Material material = new(text.fontSharedMaterial);
        material.SetColor("_OutlineColor", color);
        text.fontSharedMaterial = material;
    }
        
    public virtual bool ShouldBeVisible()
    {
        return HudManager.Instance.UseButton.gameObject.active && PlayerControl.LocalPlayer != null && 
               PlayerControl.LocalPlayer.Data is {IsDead: false} && ShipStatus.Instance != null;
    }

    public virtual bool ShouldCooldown()
    {
        return PlayerControl.LocalPlayer.moveable &&
               (!Minigame.Instance || !new[]
               {
                   // Sabotage Consoles
                   Il2CppType.Of<AirshipAuthGame>(), // Airship Reactor
                   Il2CppType.Of<AuthGame>(), // Mira Comms
                   Il2CppType.Of<BallastSabotageMinigame>(), // Submerged Reactor
                   Il2CppType.Of<DoorBreakerGame>(), // Polus Doors
                   Il2CppType.Of<DoorCardSwipeGame>(), // Airship Doors
                   Il2CppType.Of<KeypadGame>(), // Skeld and Mira Oxygen
                   Il2CppType.Of<OpenDoorsMinigame>(), // Submerged Doors
                   Il2CppType.Of<OxygenSabotageMinigame>(), // Submerged Oxygen
                   Il2CppType.Of<ReactorMinigame>(), // Skeld and Polus Reactor
                   Il2CppType.Of<SetupCamsMinigame>(), // Submerged Cameras
                   Il2CppType.Of<SwitchMinigame>(), // Lights
                   Il2CppType.Of<TuneRadioMinigame>(), // Non-Mira Comms

                   // Informational Consoles
                   Il2CppType.Of<SecurityLogGame>(), // Mira Logs
                   Il2CppType.Of<SubmarineSurvillanceMinigame>(), // Submerged Cameras
                   Il2CppType.Of<SurveillanceMinigame>(), // Skeld Cameras
                   Il2CppType.Of<PlanetSurveillanceMinigame>(), // Polus and Airship Cameras
                   Il2CppType.Of<VitalsMinigame>(), // Vitals

                   // Others
                   Il2CppType.Of<EmergencyMinigame>(), // Meeting Button
                   Il2CppType.Of<SpawnInMinigame>(), // Airship Spawn In
                   Il2CppType.Of<SubmarineSelectSpawn>(), // Submerged Spawn In
               }.Select(t => t.FullName).Contains(Minigame.Instance.GetIl2CppType().FullName)) &&
               (!HudManager.InstanceExists || !HudManager.Instance.Chat.IsOpen && !HudManager.Instance.KillOverlay.IsOpen && !HudManager.Instance.GameMenu.IsOpen) &&
               (!MapBehaviour.Instance || !MapBehaviour.Instance.IsOpenStopped) && // Admin
               !MeetingHud.Instance && !CustomPlayerMenu.Instance && !ExileController.Instance && !IntroCutscene.Instance &&
               !HudManager.Instance.shhhEmblem.isActiveAndEnabled;
    }

    public virtual bool CanUse()
    {
        return (!PlayerControl.LocalPlayer.inVent || CanUseInVents) && ShipStatus.Instance && !MeetingHud.Instance && !Minigame.Instance &&
               KillButtonManager.isActiveAndEnabled && PlayerControl.LocalPlayer.CanMove;
    }

    public virtual void Update()
    {
        KillButtonManager.renderer.enabled = true;
        GameObject.SetActive(ShouldBeVisible());

        if (MeetingHud.Instance || ExileController.Instance) CurrentTime = Cooldown;
            
        // Check if the effect is in progress
        if (CurrentTime == int.MaxValue)
        {
            KillButtonManager.TimerText.gameObject.SetActive(true);
            KillButtonManager.renderer.color = Palette.DisabledClear;
            KillButtonManager.renderer.material.SetFloat("_Desat", 1f);
            KillButtonManager.killText.alpha = Palette.DisabledClear.a;
            return;
        }

        foreach (KeyCode keycode in Keybinds)
        {
            if (Input.GetKeyDown(keycode)) PassiveButton.OnClick.Invoke();
        }
            
        // Cool the button down
        if (CurrentTime > 0)
        {
            if (ShouldCooldown()) CurrentTime -= Time.deltaTime;
        }
        else
        {
            CurrentTime = 0;
        }

        KillButtonManager.SetCoolDown(CurrentTime, Cooldown);
                
        KillButtonManager.renderer.color = CanUse() && CurrentTime <= 0 ? Palette.EnabledColor : Palette.DisabledClear;
        KillButtonManager.renderer.material.SetFloat("_Desat", CurrentTime <= 0 ? 0 : 1);
        KillButtonManager.killText.alpha = CanUse() && CurrentTime <= 0 ? Palette.EnabledColor.a : Palette.DisabledClear.a;
    }

    public virtual void DoWhileEffect() { }
        
    public virtual IEnumerator ShowEffectDuration(float duration)
    {
        for (float time = 0; time < duration; time += Time.unscaledDeltaTime)
        {
            CurrentTime = int.MaxValue;
            KillButtonManager.TimerText.text = Mathf.CeilToInt(duration - time).ToString();
            Color lerpedColor;
            if (time / duration < 0.5) lerpedColor = Color.Lerp(new Color32(30, 150, 0, 255), new Color32(255, 242, 0, 255), (time / duration) * 2);
            else lerpedColor = Color.Lerp(new Color32(255, 242, 0, 255), new Color32(255, 0, 0, 255), ((time / duration) - 0.5f) * 2);

            KillButtonManager.TimerText.color = lerpedColor;
            DoWhileEffect();
            yield return null;
        }

        CurrentTime = Cooldown;
        KillButtonManager.TimerText.color = Color.white;
    }

    public virtual void Remove()
    {
        PlayerControl.LocalPlayer.GetRoleManager().Buttons.Remove(this);
        Object.DestroyImmediate(GameObject);
    }
}