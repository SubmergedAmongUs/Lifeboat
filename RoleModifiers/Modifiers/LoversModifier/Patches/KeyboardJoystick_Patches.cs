using HarmonyLib;
using UnityEngine;

namespace Lifeboat.RoleModifiers.Modifiers.LoversModifier.Patches;

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class KeyboardJoystick_Update_Patch
{
    [HarmonyPostfix]
    public static void Postfix(KeyboardJoystick __instance)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && HudManager.Instance.Chat.IsOpen)
        {
            HudManager.Instance.Chat.Toggle();
        }
    }
}