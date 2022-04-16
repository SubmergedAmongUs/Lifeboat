using Framework.Extensions;
using HarmonyLib;
using Lifeboat.Extensions;
using UnityEngine;

namespace Lifeboat.Roles.NeutralRoles.LawyerRole.Patches;

[HarmonyPatch(typeof(CounterArea), nameof(CounterArea.UpdateCount))]
public static class CounterArea_UpdateCount_Patch
{
    public static Color BackColor = default, BodyColor, VisorColor;
        
    [HarmonyPostfix]
    public static void Postfix(CounterArea __instance, [HarmonyArgument(0)] int count)
    {
        if (count == 0) return;

        if (ShipStatus.Instance.FastRooms.TryGetValueFixed(__instance.RoomType, out PlainShipRoom room) && room.roomArea)
        {
            if (BackColor == default)
            {
                Material mat = __instance.myIcons.ToArray()[0].Cast<PooledMapIcon>().rend.material;
                BackColor = mat.GetColor("_BackColor");
                BodyColor = mat.GetColor("_BodyColor");
                VisorColor = mat.GetColor("_VisorColor");
            }
            else
            {
                foreach (PoolableBehavior behaviour in __instance.myIcons)
                {
                    Material mat = behaviour.Cast<PooledMapIcon>().rend.material;
                    mat.SetColor("_BackColor", BackColor);
                    mat.SetColor("_BodyColor", BodyColor);
                    mat.SetColor("_VisorColor", VisorColor);
                }
            }

            if (PlayerControl.LocalPlayer.GetRoleManager().MyRole is not Lawyer lawyer) return;
                
            int length = room.roomArea.OverlapCollider(MapBehaviour.Instance.countOverlay.filter, MapBehaviour.Instance.countOverlay.buffer);
            for (int i = 0; i < length; i++)
            {
                Collider2D collider2D = MapBehaviour.Instance.countOverlay.buffer[i];

                if (collider2D.GetComponent<PlayerControl>() is { } player && player.PlayerId == lawyer.Target.PlayerId ||
                    collider2D.GetComponent<DeadBody>() is { } body && body.ParentId == lawyer.Target.PlayerId)
                {
                    Material mat = __instance.myIcons.ToArray()[0].Cast<PooledMapIcon>().rend.material;
                    mat.SetColor("_BackColor", new Color32(63, 130, 90, 255));
                    mat.SetColor("_BodyColor", Lawyer.Settings.HeaderColor);
                    mat.SetColor("_VisorColor", Palette.VisorColor);
                    return;
                }
            }
        }
    }
}