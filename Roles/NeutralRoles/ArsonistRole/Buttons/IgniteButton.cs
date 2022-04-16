using Framework.Utilities;
using Lifeboat.Buttons;

namespace Lifeboat.Roles.NeutralRoles.ArsonistRole.Buttons;

public sealed class IgniteButton : BaseButton
{
    public override float Cooldown => 5f;
    private Arsonist Arsonist { get; set; }
    public IgniteButton(Arsonist arsonist)
    {
        Arsonist = arsonist;
        SetSprite(ResourceManager.GetSprite("Ignite", 265f));
        SetPosition(AspectPosition.EdgeAlignments.RightBottom);
    }

    public override void OnClick()
    {
        Arsonist.ArsonistWin();
    }
}