using Lifeboat.Roles;
using UnityEngine;

namespace Lifeboat.Extensions;

public static class RoleExtensions
{
    private static readonly int Color = Shader.PropertyToID("_Color");

    public static void DefaultIntroCutscene(this BaseRole role, IntroCutscene introCutscene, string impostorText = "")
    {
        introCutscene.Title.text = role.RoleName;
        introCutscene.Title.color = role.Color;

        introCutscene.BackgroundBar.material.SetColor(Color, role.Color);
            
        introCutscene.ImpostorText.gameObject.SetActive(true);
        introCutscene.ImpostorText.text = impostorText;
    }

    public static bool Is<T>(this PlayerControl player, out T role) where T : BaseRole
    {
        BaseRole playerRole = player.GetRoleManager().MyRole;
        role = null;
        if (playerRole is not T baseRole) return false;
        role = baseRole;
        return true;
    }
}