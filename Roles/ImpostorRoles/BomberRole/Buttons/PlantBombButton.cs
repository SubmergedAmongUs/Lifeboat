using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Localization.Languages;
using Framework.Utilities;
using Hazel;
using Lifeboat.Buttons;
using Lifeboat.Enums;
using TMPro;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles.BomberRole.Buttons;

public sealed class PlantBombButton : TargetedButton<PlayerControl>
{
    public Bomber Bomber { get; }

    public PlantBombButton(Bomber bomber)
    {
        SetSprite(ResourceManager.SpriteCache["Bomb"]);
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        Bomber = bomber;
        Cooldown = PlayerControl.GameOptions.KillCooldown;
            
        if (PlayerControl.GameOptions.MapId == 5) CurrentTime = Cooldown;
        else CurrentTime = 10;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Bomber_Button_PlantBomb));
        SetTextColor(Bomber.Settings.HeaderColor);

        TextMeshPro text = GameObject.transform.Find("Text_TMP").GetComponent<TextMeshPro>();
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x * 1.5f, rect.sizeDelta.y);
    }
        
    public override float EffectDuration => Bomber.RevealDelay;

    public override bool ShowOutlineDuringEffect => true;

    public override void DoWhileEffect() => SetOutline(Target, ShowOutlineDuringEffect, Bomber.Settings.HeaderColor);

    public override PlayerControl GetClosest()
    {
        IEnumerable<PlayerControl> validPlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => !p.Data.Disconnected && !p.AmOwner && !p.Data.IsDead && !p.Data.IsImpostor);

        float maxDistance = TargetRange();
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

    public override void OnClick()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRpcCalls.Bomber_PlantBomb, SendOption.Reliable);
        writer.Write(Target.PlayerId);
        writer.EndMessage();

        Bomber.PlantBomb(Target);
        PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.GameOptions.KillCooldown + EffectDuration + Bomber.FuseDuration);
    }

    public override IEnumerator ShowEffectDuration(float duration)
    {
        yield return base.ShowEffectDuration(duration);
            
        CurrentTime = Cooldown + Bomber.FuseDuration;
        PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.GameOptions.KillCooldown + Bomber.FuseDuration);
    }
}