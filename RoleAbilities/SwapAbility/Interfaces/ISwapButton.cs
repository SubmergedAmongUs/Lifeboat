namespace Lifeboat.RoleAbilities.SwapAbility.Interfaces;

public interface ISwapButton
{
    PlayerVoteArea Parent { get; }
    int GetInstanceID();
}