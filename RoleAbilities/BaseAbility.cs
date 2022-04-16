using Lifeboat.Roles;

namespace Lifeboat.RoleAbilities;

public abstract class BaseAbility
{
    public BaseRole Owner { get; set; }
        
    public BaseAbility(BaseRole owner)
    {
        Owner = owner;
    }
}