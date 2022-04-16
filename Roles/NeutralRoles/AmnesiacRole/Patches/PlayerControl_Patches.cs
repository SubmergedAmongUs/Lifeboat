using System;
using Framework.Extensions;
using HarmonyLib;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.MonoBehaviours;

namespace Lifeboat.Roles.NeutralRoles.AmnesiacRole.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    public static class PlayerControl_HandleRpc_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            switch ((CustomRpcCalls) callId)
            {
                case CustomRpcCalls.Amnesiac_Remember:
                    string type = reader.ReadString();
                    if (Activator.CreateInstance(Type.GetType(type)!) is not BaseRole newRole)
                    {
                        LifeboatPlugin.Log.LogError("Couldn't create new amnesiac role from type: " + type);
                        return;
                    }
                    RoleManager manager = __instance.GetRoleManager();
                    newRole.Deserialize(reader);
                    newRole.PreviousRole = $"{manager.MyRole.PreviousRole}<color=#{manager.MyRole.Color.ToRGBAString()}>{manager.MyRole.RoleName}</color> -> ";
                    manager.MyRole = newRole;
                    newRole.Start();
                    return;
            }
        }
    }
}