using System;
using System.Collections.Generic;
using System.Linq;
using Framework.CustomOptions.Utils;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Hazel;
using Lifeboat.Enums;
using Lifeboat.Events;
using Lifeboat.Extensions;
using Lifeboat.GameOptions;
using Lifeboat.Patches;
using Lifeboat.RoleModifiers.Modifiers.LoversModifier;
using Lifeboat.Roles;
using Lifeboat.Roles.CrewmateRoles;
using Lifeboat.Roles.CrewmateRoles.MedicRole;
using Lifeboat.Roles.ImpostorRoles.AssassinRole;
using Lifeboat.Roles.NeutralRoles.LawyerRole;
using Lifeboat.Utils;
using TMPro;
using UnityEngine;

namespace Lifeboat.RoleAbilities.GuessAbility;

public sealed class GuessAbility : BaseAbility
{
    public int RemainingKills { get; set; } = (int) (Assassin.MaxKillsPerMeeting == 0 ? 200 : Assassin.MaxKillsPerMeeting);
    public GameObject Overlay { get; set; }
    public GameData.PlayerInfo TargetPlayer { get; set; }

    private List<Type> Roles { get; set; }
    private int SelectedRoleIdx { get; set; }
    public Type CurrentRole => Roles[SelectedRoleIdx];
        
    public GuessAbility(BaseRole myRole) : base(myRole)
    {
    }
        
    public void Start()
    {
        if (Owner.Owner.AmOwner)
        {
            MeetingHudEvents.OnMeetingStart += HandleMeetingHudStart;
        }
    }

    public void Update()
    {
        if (Owner.Owner.AmOwner && Overlay.IsThere() && TargetPlayer is {Disconnected: true})
        {
            Overlay.gameObject.Destroy();
            Overlay = null;
            TargetPlayer = null;
        }
    }
        
    public void OnDestroy()
    {
        MeetingHudEvents.OnMeetingStart -= HandleMeetingHudStart;
    }
        
    public void HandleMeetingHudStart(MeetingHud meetingHud)
    {
        RemainingKills = (int) Assassin.MaxKillsPerMeeting;
    }
        
    public void Guess(byte playerId)
    {
        RoleManager roleManager = GameData.Instance.GetPlayerById(playerId).Object!?.GetRoleManager();
        byte targetId = roleManager!?.MyRole.GetType() == CurrentRole || roleManager.MyModifier?.GetType() == CurrentRole ? playerId : Owner.Owner.PlayerId;
            
        MessageWriter writer = AmongUsClient.Instance.StartRpc(Owner.Owner.NetId, (byte) CustomRpcCalls.Assassin_Kill, SendOption.Reliable);
        writer.Write(targetId);
        writer.EndMessage();
            
        AssassinKill(targetId);
    }
        
    public void ShowGuessWindow(Transform meetingTransform, byte playerId)
    {
        Dictionary<Type, string> roleToStringIdDict = new(BaseRole.RoleTypeToStringID);
        Dictionary<Type, Color32> roleToColorDict = new(BaseRole.RoleTypeToColor);

        if (Assassin.CanGuessLovers)
        {
            roleToStringIdDict.Add(typeof(Lovers), nameof(English.Lifeboat_Lovers_UI_LoverSingular));
            roleToColorDict.Add(typeof(Lovers), Lovers.Settings.HeaderColor);
        }

        if (!Assassin.CanGuessCrewmate)
        {
            roleToStringIdDict.Remove(typeof(Crewmate));
            roleToColorDict.Remove(typeof(Crewmate));
        }

        if (GeneralOptions.AllImpsCanAssassinate)
        {
            roleToStringIdDict.Remove(typeof(Assassin));
            roleToColorDict.Remove(typeof(Assassin));
        }

        Roles = roleToStringIdDict.OrderBy(t => LanguageProvider.GetLocalizedString(t.Value), 
            StringComparer.CurrentCultureIgnoreCase).Select(s => s.Key).ToList();
        SelectedRoleIdx = 0;
            
        PlayerControl player = GameData.Instance.GetPlayerById(playerId).Object;
        
        Transform profileWindow = AmongGUI.MakeGameObject("Role Guesser", meetingTransform, -15f).transform;
        Overlay = profileWindow.gameObject;
        TargetPlayer = GameData.Instance.GetPlayerById(playerId);

        // Background
        GameObject background = AmongGUI.MakeBackground(5f, 3.5f, profileWindow, 5f);
         
        // Click Masks
        AmongGUI.MakeClickMaskOnObject(background.transform);
        AmongGUI.MakeClickMaskAsChild(background.transform, 0.1f).OnClick.AddListener((Action) (() => GameObject.Destroy(profileWindow.gameObject)));
            
        // Title
        TextMeshPro title = AmongGUI.MakeText(profileWindow, LanguageProvider.Current.Lifeboat_Assassin_UI_Title);
        title.transform.localPosition = new Vector3(0f, 1.35f, -1);

        TextMeshPro mainText = AmongGUI.MakeText(profileWindow, string.Format(LanguageProvider.Current.Lifeboat_Assassin_UI_Text, 
            player.Data.PlayerName, roleToColorDict[CurrentRole].ToRGBAString(), LanguageProvider.GetLocalizedString(roleToStringIdDict[CurrentRole])));
        mainText.transform.localPosition = new Vector3(0f, 0.1f, -1);
            
        // Close Button
        PassiveButton closeButton = AmongGUI.MakeTextButton($"<color=red>{LanguageProvider.Current.Lifeboat_Assassin_UI_Close}</color>", 1.2f, 
            parent: profileWindow, hoverChangeColor: true);
        closeButton.transform.localPosition = new Vector3(-1f, -1.4f, -1);
        closeButton.OnClick.AddListener((Action) (() =>
        {
            if (!PassiveButtonManager.Instance.controller.CheckHover(closeButton.Colliders[0])) return;
            UnityEngine.Object.Destroy(profileWindow.gameObject);
            Overlay = null;
            TargetPlayer = null;
        }));
            
        // Guess Button
        PassiveButton newButton = AmongGUI.MakeTextButton($"<color=green>{LanguageProvider.Current.Lifeboat_Assassin_UI_Guess}</color>", 1.2f, 
            parent: profileWindow, hoverChangeColor: true);
        newButton.transform.localPosition = new Vector3(1f, -1.4f, -1);
        newButton.OnClick.AddListener((Action) (() =>
        {
            if (!PassiveButtonManager.Instance.controller.CheckHover(newButton.Colliders[0])) return;
            UnityEngine.Object.Destroy(profileWindow.gameObject);
            Overlay = null;
            TargetPlayer = null;
            Guess(playerId);
        }));
            
        // Previous Button
        PassiveButton previousButton = AmongGUI.MakeTextButton("<", 0.4f, fontSize: 2, parent: profileWindow, hoverChangeColor: true);
        previousButton.transform.localPosition = new Vector3(-0.225f, -0.65f, -1);
        previousButton.OnClick.AddListener((Action) (() =>
        {
            SelectedRoleIdx = (SelectedRoleIdx - 1) % Roles.Count;
            if (SelectedRoleIdx == -1) SelectedRoleIdx = Roles.Count - 1;
                
            mainText.text = string.Format(LanguageProvider.Current.Lifeboat_Assassin_UI_Text, player.Data.PlayerName, 
                roleToColorDict[CurrentRole].ToRGBAString(), LanguageProvider.GetLocalizedString(roleToStringIdDict[CurrentRole]));
        }));
            
        // Next Button
        PassiveButton nextButton = AmongGUI.MakeTextButton(">", 0.4f, fontSize: 2, parent: profileWindow, hoverChangeColor: true);
        nextButton.transform.localPosition = new Vector3(0.225f, -0.65f, -1);
        nextButton.OnClick.AddListener((Action) (() =>
        {
            SelectedRoleIdx = (SelectedRoleIdx + 1) % Roles.Count;
                
            mainText.text = string.Format(LanguageProvider.Current.Lifeboat_Assassin_UI_Text, player.Data.PlayerName, 
                roleToColorDict[CurrentRole].ToRGBAString(), LanguageProvider.GetLocalizedString(roleToStringIdDict[CurrentRole]));
        }));
    }

    public void AssassinKill(byte targetId)
    {
        PlayerVoteArea voteArea = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == targetId);
        PlayerControl target = GameData.Instance.GetPlayerById(targetId).Object;
            
        RemainingKills--;
        ShipStatus_CheckEndCriteria_Patch.Timeout = 50;
            
        if (Medic.CheckMedicProtection(Owner.Owner, target, true)) return;
        if (Lawyer.CheckLawyerProtection(Owner.Owner, target, true)) return;

        PlayerControlEvents.OnPlayerCustomMurder?.Invoke(Owner.Owner, target);

        if (target.AmOwner)
        {
            HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
            target.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
            target.RpcSetScanner(false);
        }
            
        SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);
        HudManager.Instance.KillOverlay.ShowKillAnimation(target.Data, target.Data);
        target.Die(DeathReason.Kill);
        target.GetRoleManager().KilledBy = Owner.Owner.PlayerId;

        MeetingHud meetingHud = MeetingHud.Instance;
        if (target.AmOwner) meetingHud.SetForegroundForDead();

        if (voteArea == null) return;
        if (voteArea.DidVote) voteArea.UnsetVote();
        voteArea.AmDead = true;
        voteArea.Overlay.gameObject.SetActive(true);
        voteArea.Overlay.color = Color.white;
        voteArea.XMark.gameObject.SetActive(true);
        voteArea.XMark.transform.localScale = Vector3.one;
        bool amHost = AmongUsClient.Instance.AmHost;
        foreach (PlayerVoteArea playerVoteArea in meetingHud.playerStates)
        {
            if (playerVoteArea.VotedFor != target.PlayerId) continue;
            playerVoteArea.UnsetVote();
            PlayerControl voteAreaPlayer = GameData.Instance.GetPlayerById(playerVoteArea.TargetPlayerId).Object;
            if (!voteAreaPlayer.AmOwner) continue;
            meetingHud.ClearVote();
        }

        if (!amHost) return;
        meetingHud.CheckForEndVoting();
    }
}