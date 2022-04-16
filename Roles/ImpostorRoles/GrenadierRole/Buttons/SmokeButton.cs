using System.Collections.Generic;
using System.Linq;
using Framework.Extensions;
using Framework.Localization.Languages;
using Hazel;
using Lifeboat.Buttons;
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles.GrenadierRole.Buttons;

public sealed class SmokeButton : BaseButton
{
    public SmokeButton(Grenadier grenadier)
    {
        Grenadier = grenadier;
        SetSprite("Smoke", 200);

        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
        CurrentTime = Cooldown = Grenadier.SmokeCooldown;
    }

    public override void SetupButtonText()
    {
        SetTextTransform();
        SetTextTranslation(nameof(English.Lifeboat_Grenadier_Button_Smoke));
        SetTextColor(new Color32(114, 143, 61, 255));
    }

    public Grenadier Grenadier { get; set; }
    public override float EffectDuration => Grenadier.SmokeDuration;

    public override void OnClick()
    {
        IEnumerable<PlayerControl> validPlayers = PlayerControl.AllPlayerControls.ToArray().Where(p => !p.Data.Disconnected && !p.AmOwner);
        List<byte> smokedPlayers = new();
            
        float maxDistance = 5.33333333333f;
            
        Vector2 myPos = PlayerControl.LocalPlayer.GetTruePosition();

        foreach (PlayerControl player in validPlayers)
        {
            Vector2 deltaVector2 = player.GetTruePosition() - myPos;
            float magnitude = deltaVector2.magnitude;
            if (magnitude <= maxDistance && (Grenadier.SmokeThroughWalls || !PhysicsHelpers.AnyNonTriggersBetween(myPos, deltaVector2.normalized, magnitude, Constants.ShipAndObjectsMask)))
            {
                smokedPlayers.Add(player.PlayerId);

                if (!player.Data.IsImpostor)
                {
                    Grenadier.Owner.GetRoleManager().StartCoroutine(Grenadier.CoDoNameOverride(player));
                }
            }
        }

        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Grenadier_Smoke, SendOption.Reliable);
        writer.WriteBytesAndSize(smokedPlayers.ToArray());
        writer.EndMessage();
            
        Grenadier.Smoke(0.2f);
    }
}