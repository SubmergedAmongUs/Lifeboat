using System.Collections;
using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Hazel;
using Lifeboat.Attributes;
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.Roles.NeutralRoles.GlitchRole.Buttons;
using Lifeboat.WinScreen;
using UnityEngine;

namespace Lifeboat.Roles.NeutralRoles.GlitchRole;

[OptionHeader(nameof(English.Lifeboat_Glitch))]
public sealed class Glitch : BaseRole
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_NeutralRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Glitch), "Glitch")] 
    public static float GlitchAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 18),
        HeaderColor = new Color32(64, 235, 115, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(GlitchAmount, Alignment.Neutral),
    };

    [NumberOption(nameof(English.Lifeboat_Glitch_GameOptions_MimicDuration), "Mimic Duration", 10, 
        3, 25, 1f, false, "{0}s")] 
    public static float MorphDuration = 10;
        
    [NumberOption(nameof(English.Lifeboat_Glitch_GameOptions_MimicCooldown), "Mimic Cooldown", 9, 
        5, 60, 2.5f, false, "{0:0.0}s")] 
    public static float MorphCooldown = 25;

    [NumberOption(nameof(English.Lifeboat_Glitch_GameOptions_HackDuration), "Hack Duration", 8, 
        3, 25, 1f, false, "{0}s")] 
    public static float HackDuration = 10f;
        
    [NumberOption(nameof(English.Lifeboat_Glitch_GameOptions_HackCooldown), "Hack Cooldown", 7, 
        5, 60, 2.5f, false, "{0:0.0}s")] 
    public static float HackCooldown = 25;

    #endregion

    public override string RoleStringID => nameof(English.Lifeboat_Glitch);
    public override Color32 Color => Settings.HeaderColor;
    public override Alignment Alignment => Alignment.Neutral;

    public PlayerControl Hacked { get; set; }
    public bool WasHacked { get; set; }
    public PlayerControl Sampled { get; set; }

    public override void Start()
    {
        if (!Owner.AmOwner) return;
            
        new GlitchKillButton();
        new HackButton(this);
            
        new GlitchSampleButton(this);
        new MimicButton(this);
    }

    public override void Update()
    {
        if (Hacked && Hacked.AmOwner)
        {
            WasHacked = true;
            HudManager.Instance.SetHudActive(false);
                
            if (Minigame.Instance) Minigame.Instance.Close();
            if (Minigame.Instance) Minigame.Instance.Close();

            if (MeetingHud.Instance) Hacked = null;
        }
        else if (WasHacked)
        {
            HudManager.Instance.SetHudActive(!MeetingHud.Instance && !ExileController.Instance);
            WasHacked = false;
        }
    }
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Glitch_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }
        
    public void Morph(byte id, float duration)
    {
        GameData.PlayerInfo sampledData = GameData.Instance.GetPlayerById(id);
        Sampled = sampledData.Object;

        AppearanceModification modification = new()
        {
            Data = new AppearanceData
            {
                Name = Sampled.nameText.text,
                ColorId = sampledData.ColorId,
                HatId = sampledData.HatId,
                SkinId = sampledData.SkinId,
                PetId = sampledData.PetId,
            },
            ModificationMask = AppearanceModification.Overrides.Name |
                               AppearanceModification.Overrides.ColorId |
                               AppearanceModification.Overrides.HatId |
                               AppearanceModification.Overrides.SkinId |
                               AppearanceModification.Overrides.PetId,
            Priority = 1,
            Timer = duration
        };
            
        Owner.GetRoleManager().AppearanceManager.Modifications.Add(modification);
    }
        
    public void GlitchWin()
    {
        TempWinData winData = new()
        {
            SubtitleStringID = nameof(English.Lifeboat_WinReason_Glitch),
            Args = new[] {Owner.Data.PlayerName, Color.ToRGBAString()},
            ShowNames = true,
            WinnerBackgroundBarColor = Color,
            LoserBackgroundBarColor = Color,
            AudioStinger = TempWinData.Stinger.Impostor,
            WinnerIds = new[] {Owner.PlayerId}
        };

        WinScreenNetworking.RpcCustomEndGame(winData);
    }
        
    public void RpcHack(PlayerControl target)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Glitch_Hack, SendOption.Reliable);
        writer.Write(target.PlayerId);
        writer.EndMessage();

        Owner.GetRoleManager().StartCoroutine(CoHackPlayer(target.PlayerId));
    }

    public IEnumerator CoHackPlayer(byte playerId)
    {
        Hacked = GameData.Instance.GetPlayerById(playerId).Object;
        yield return new WaitForSeconds(HackDuration);
        Hacked = null;
    }
}