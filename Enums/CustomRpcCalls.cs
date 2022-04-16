namespace Lifeboat.Enums;

public enum CustomRpcCalls
{
    General_SetRoles = 200,
    General_EndGame,

    Altruist_Revive,
    Mayor_Vote,
    Medic_Monitor,
    Oracle_Predict,
    Swapper_Swap,
    TimeLord_Rewind,

    Assassin_Kill,
    Bomber_PlantBomb,
    Bomber_Explode,
    Bomber_ExplodeAtMeeting,
    Camouflager_Camouflage,
    Grenadier_Smoke,
    Janitor_Clean,
    Miner_PlaceVent,
    Morphling_Morph,
    Poisoner_Poison,
    Poisoner_DieToPoison,
    Swooper_Swoop,
    Undertaker_Drag,
    Undertaker_Drop,

    Executioner_SetTarget,
    Executioner_BecomeJester,
    Glitch_Morph,
    Glitch_Hack,
    Lawyer_SetClient,
    Lawyer_BecomeJester,

    Lovers_SendFreeChat,
    Lovers_SendQuickChat,
}