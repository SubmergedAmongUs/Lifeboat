using System.Linq;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Framework.Utilities;
using Hazel;
using Lifeboat.Buttons;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.Roles.CrewmateRoles.SheriffRole;
using Lifeboat.Roles.CrewmateRoles.SheriffRole.Buttons;
using Lifeboat.Roles.NeutralRoles.GlitchRole;
using Lifeboat.Roles.NeutralRoles.GlitchRole.Buttons;
using Lifeboat.Utils;
using TMPro;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles.BomberRole.Buttons;

public sealed class ThrowBombButton : TargetedButton<PlayerControl>
{
    public Bomber Bomber { get; }

    public ThrowBombButton(Bomber bomber)
    {
        SetSprite(ResourceManager.SpriteCache["Bomb"]);

        AspectPosition.Alignment = AspectPosition.EdgeAlignments.Bottom;
        AspectPosition.DistanceFromEdge = new Vector3(0, 1.3f, -5f);
        AspectPosition.updateAlways = true;
            
        Bomber = bomber;
        Cooldown = CurrentTime = 0.1f;
        BombDuration = Bomber.FuseDuration;

        RoleManager roleManager = PlayerControl.LocalPlayer.GetRoleManager();
            
        switch (roleManager.MyRole)
        {
            case Glitch:
                roleManager.Buttons.First(b => b is GlitchKillButton).Remove();
                break;
                
            case Sheriff:
                roleManager.Buttons.First(b => b is SheriffKillButton).Remove();
                break;
        }

        roleManager.StartCoroutine(UsefulMethods.CoFlashScreen(new Color32(255, 207, 112, 77)));
        UsefulMethods.ShowTextToast(string.Format(LanguageProvider.Current.Lifeboat_Bomber_PlantWarning, Bomber.FuseDuration), 3);
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Bomber_Button_ThrowBomb));
        SetTextColor(Bomber.Settings.HeaderColor);

        TextMeshPro text = GameObject.transform.Find("Text_TMP").GetComponent<TextMeshPro>();
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x * 1.5f, rect.sizeDelta.y);
    }
        
    public override bool IsModifierButton => true;
    public override KeyCode[] Keybinds => new[] {KeyCode.Q};
        
    public float BombDuration { get; set; }
    public float AlertTicker { get; set; }
        
    public override PlayerControl GetClosest() => UsefulMethods.GetClosestPlayer(TargetRange(), false);

    public override void OnClick()
    {
        MurderPlayer(Target.GetRoleManager().MyRole is Bomber ? PlayerControl.LocalPlayer : Target);
        Remove();
    }

    public void MurderPlayer(PlayerControl target)
    {
        Bomber.Explode(PlayerControl.LocalPlayer, target);
        Bomber.CannotReport.Add(target.PlayerId);

        MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Bomber_Explode);
        messageWriter.Write(Bomber.Owner.PlayerId);
        messageWriter.Write(target.PlayerId);
        messageWriter.EndMessage();
    }

    public void ExplodeAtMeeting()
    {
        Bomber.ExplodeAtMeeting(PlayerControl.LocalPlayer);

        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRpcCalls.Bomber_ExplodeAtMeeting);
        writer.Write(Bomber.Owner.PlayerId);
        writer.EndMessage();
    }

    public override void Remove()
    {
        switch (PlayerControl.LocalPlayer.GetRoleManager().MyRole)
        {
            case Glitch:
                new GlitchKillButton(-1);
                break;
                
            case Sheriff s:
                new SheriffKillButton(s);
                break;
        }
            
        base.Remove();
    }

    public override void Update()
    {
        base.Update();

        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            base.Remove();
            return;
        }

        if (MeetingHud.Instance)
        {
            ExplodeAtMeeting();
            Remove();
            return;
        }
            
        float deltaTime = Time.deltaTime;
        BombDuration -= deltaTime;
        AlertTicker += deltaTime;

        if (BombDuration <= 0)
        {
            MurderPlayer(PlayerControl.LocalPlayer);
            Remove();
            return;
        }

        // Alert the target every 5 seconds
        if (AlertTicker > 5f)
        {
            AlertTicker = 0;
                
            PlayerControl.LocalPlayer.GetRoleManager().StartCoroutine(UsefulMethods.CoFlashScreen(new Color32(255, 207, 112, 77)));
            UsefulMethods.ShowTextToast(string.Format(LanguageProvider.Current.Lifeboat_Bomber_TimerWarning, Mathf.CeilToInt(BombDuration)), 1.25f);
        }
            
        KillButtonManager.TimerText.gameObject.SetActive(true);
        KillButtonManager.TimerText.text = Mathf.CeilToInt(BombDuration).ToString();

        Color lerpedColor = BombDuration / Bomber.FuseDuration < 0.5 
            ? Color.Lerp(new Color32(255, 0, 0, 255), new Color32(255, 242, 0, 255), BombDuration / Bomber.FuseDuration * 2) 
            : Color.Lerp(new Color32(255, 242, 0, 255), new Color32(30, 150, 0, 255), (BombDuration / Bomber.FuseDuration - 0.5f) * 2);

        KillButtonManager.TimerText.color = lerpedColor;
    }
}