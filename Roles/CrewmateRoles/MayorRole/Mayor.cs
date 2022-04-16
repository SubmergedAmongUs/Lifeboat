using System.Collections.Generic;
using System.Linq;
using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Hazel;
using Lifeboat.Attributes;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.GameOptions;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.MayorRole;

[OptionHeader(nameof(English.Lifeboat_Mayor))]
public sealed class Mayor : Crewmate
{
    #region Option

    [OptionHeader(nameof(English.Lifeboat_GameOptions_CrewmateRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Mayor), "Mayor")] 
    public static float MayorAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 97),
        HeaderColor = new Color32(167, 64, 214, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(MayorAmount, Alignment.Crewmate),
    };

    [NumberOption(nameof(English.Lifeboat_Mayor_GameOptions_InitialVoteBank), "Initial Vote Bank", 10,
        0, 5, 1, false, "{0}")] 
    public static float InitialVoteBank = 2;
        
    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Mayor);
    public override Color32 Color => Settings.HeaderColor;

    public override bool IsJousting() => PlayerControl.AllPlayerControls.ToArray()
        .Count(p => p.IsThere() && p.Data is {IsDead: false, Disconnected: false, IsImpostor: true}) <= 1;

    public bool VotedThisRound { get; set; }
    public int VoteBank { get; set; } = (int) InitialVoteBank;
    public List<byte> Votes { get; set; }= new();
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Mayor_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }
        
    public void RpcVote(byte playerId)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Mayor_Vote, SendOption.Reliable);
        writer.Write(playerId);
        writer.EndMessage();
            
        Vote(playerId);
    }
        
    public void Vote(byte playerId)
    {
        Votes.Add(playerId);
    }
}