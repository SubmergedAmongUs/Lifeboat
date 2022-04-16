using Lifeboat.Enums;
using Lifeboat.GameOptions;
using Lifeboat.RoleAbilities.GuessAbility;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles;

public class Impostor : BaseRole
{
    public override string RoleStringID => nameof(StringNames.Impostor);
    public override Color32 Color => Palette.ImpostorRed;
    public override Alignment Alignment => Alignment.Impostor;

    public GuessAbility GuessAbility { get; set; }

    public override void Start()
    {
        if (GeneralOptions.AllImpsCanAssassinate)
        {
            GuessAbility = new GuessAbility(this);
            GuessAbility.Start();
                
            Abilities.Add(GuessAbility);
        }
    }

    public override void Update()
    {
        GuessAbility?.Update();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        GuessAbility?.OnDestroy();
    }

    public override void OnFailedNonMeetingKill()
    {
        Owner.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
    }
}