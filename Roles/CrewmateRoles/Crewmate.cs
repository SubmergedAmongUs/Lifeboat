using Lifeboat.Enums;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles;

public class Crewmate : BaseRole
{
    public override string RoleStringID => nameof(StringNames.Crewmate);
    public override Color32 Color => UnityEngine.Color.white;
    public override Alignment Alignment => Alignment.Crewmate;
}