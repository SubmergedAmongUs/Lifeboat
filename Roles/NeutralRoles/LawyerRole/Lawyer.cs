using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Framework.Utilities;
using HarmonyLib;
using Hazel;
using Lifeboat.Attributes;
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Events;
using Lifeboat.Extensions;
using Lifeboat.Roles.NeutralRoles.JesterRole;
using Lifeboat.Utils;
using Lifeboat.WinScreen;
using UnityEngine;

namespace Lifeboat.Roles.NeutralRoles.LawyerRole;

[OptionHeader(nameof(English.Lifeboat_Lawyer))]
public sealed class Lawyer : BaseRole
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_NeutralRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Lawyer), "Lawyer")] 
    public static float LawyerAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 16),
        HeaderColor = new Color32(112, 185, 141, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(LawyerAmount, Alignment.Neutral),
    };
        
    //[ToggleOption(nameof(English.Lifeboat_Lawyer_GameOptions_CanSeeVotes), "Lawyer_CanSeeVotes", 10)]
    //public static bool CanSeeVotes = true;

    #endregion

    public override string RoleStringID => nameof(English.Lifeboat_Lawyer);
    public override Color32 Color => Settings.HeaderColor;
    public override Alignment Alignment => Alignment.Neutral;
        
    public GameData.PlayerInfo Target { get; set; }
    public Alignment TargetAlignment { get; set; }
    public bool TargetSet { get; set; }
    public bool HasShield { get; set; } = true;
    public List<NameOverride> NameOverrides { get; } = new();
        
    public ArrowBehaviour Arrow { get; set; }
    public DeadBody TargetBody { get; set; }
        
    public override void Start()
    {
        WinScreenNetworking.ModifyWinData += RegisterAdditionalWinners;
            
        Abilities.Add(new LawyerSwapAbility(this, 10));
            
        if (Owner.AmOwner)
        {
            PlayerControlEvents.OnPlayerMurder += HandlePlayerMurder;
            PlayerControlEvents.OnPlayerCustomMurder += HandlePlayerCustomMurder;
            PlayerControlEvents.OnPlayerExile += HandlePlayerExile;
            MeetingHudEvents.OnMeetingStart += HandleMeetingStart;
            PlayerControlEvents.OnPlayerDisconnect += HandlePlayerDisconnect;

            if (!TargetSet)
            {
                PlayerControl[] allPlayers = PlayerControl.AllPlayerControls.ToArray();

                PlayerControl targetControl = allPlayers
                    .Where(p => p.IsThere() && !p.AmOwner && p.Data != null && p.GetRoleManager()?.MyRole != null)
                    .ToArray().Shuffle().FirstOrDefault() ?? PlayerControl.LocalPlayer;
                Target = targetControl.Data;
                TargetAlignment = targetControl.GetRoleManager().MyRole.Alignment;
                TargetSet = true;
                MessageWriter writer = AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Lawyer_SetClient, SendOption.Reliable);
                writer.Write(Target.PlayerId);
                writer.EndMessage();
            }
                
            GameObject arrow = MapLoader.Skeld.CommonTasks
                .First(t => t!?.GetComponentInChildren<ArrowBehaviour>(true))
                .GetComponentInChildren<ArrowBehaviour>(true).gameObject;
            ArrowBehaviour arrowBehaviour = GameObject.Instantiate(arrow).GetComponent<ArrowBehaviour>();
            arrowBehaviour.gameObject.SetActive(true);
            arrowBehaviour.transform.parent = PlayerControl.LocalPlayer.transform;
            arrowBehaviour.image = arrowBehaviour.GetComponent<SpriteRenderer>();
            arrowBehaviour.image.color = Color;
            Arrow = arrowBehaviour;
        }
            
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            RoleManager manager = playerControl!?.GetRoleManager();
            if (manager)
            {
                NameOverride nameOverride = new(LawyerNameOverride, 10);
                manager.NameOverrides.Add(nameOverride);
                NameOverrides.Add(nameOverride);
            }
        }
    }
        
    public override void Update()
    {
        if (!Owner.AmOwner) return;
        if (!TargetSet || !ShipStatus.Instance || !ShipStatus.Instance.enabled || Owner.Data == null) return;

        if (Arrow != null && Target != null && Target.Object.IsThere())
        {
            if (Owner.Data.IsDead) Arrow.gameObject.SetActive(false);
            else Arrow.target = !TargetBody ? Target.Object.transform.position : TargetBody.transform.position;
        }
    }
        
    public override void OnDestroy()
    {
        WinScreenNetworking.ModifyWinData -= RegisterAdditionalWinners;
        PlayerControlEvents.OnPlayerMurder -= HandlePlayerMurder;
        PlayerControlEvents.OnPlayerCustomMurder -= HandlePlayerCustomMurder;
        PlayerControlEvents.OnPlayerExile -= HandlePlayerExile;
        MeetingHudEvents.OnMeetingStart -= HandleMeetingStart;
        PlayerControlEvents.OnPlayerDisconnect -= HandlePlayerDisconnect;
            
        base.OnDestroy();
        foreach (NameOverride nameOverride in NameOverrides)
        {
            nameOverride.Dispose();
        }
    }

    public void HandlePlayerMurder(PlayerControl _, PlayerControl target)
    {
        if (target.PlayerId == Target.PlayerId)
        {
            Owner.GetRoleManager().StartCoroutine(CoHandlePlayerMurder());

            if (!Owner.Data.IsDead && TargetAlignment == Alignment.Neutral)
            {
                BecomeJester();
                AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Lawyer_BecomeJester).EndMessage();

                MeetingHudEvents.OnMeetingStart += _ => Arrow.gameObject.SetActive(false);
            }
        }
    }

    public IEnumerator CoHandlePlayerMurder()
    {
        while ((TargetBody = GameObject.FindObjectsOfType<DeadBody>().FirstOrDefault(d => d.ParentId == Target.PlayerId)) is null) yield return null;
    }

    public void HandlePlayerCustomMurder(PlayerControl _, PlayerControl target) => HandlePlayerExile(target);

    public void HandlePlayerExile(PlayerControl player)
    {
        if (player.PlayerId == Target.PlayerId)
        {
            Arrow.gameObject.SetActive(false);
                
            if (!Owner.Data.IsDead && TargetAlignment == Alignment.Neutral)
            {
                BecomeJester();
                AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Lawyer_BecomeJester).EndMessage();
            }
        }
    }
        
    public void HandleMeetingStart(MeetingHud _)
    {
        if (Target.IsDead)
        {
            Arrow.gameObject.SetActive(false);
        }
    }

    public void HandlePlayerDisconnect(GameData.PlayerInfo player)
    {
        if (player.PlayerId == Target.PlayerId)
        {
            if (TargetAlignment == Alignment.Neutral)
            {
                UsefulMethods.ShowTextToast(string.Format(LanguageProvider.Current.Lifeboat_Lawyer_Toast_ClientDcdTurnJest, 
                    Color, Jester.Settings.HeaderColor, LanguageProvider.Current.Lifeboat_Jester), 2.5f);
                    
                BecomeJester();
                AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Lawyer_BecomeJester).EndMessage();
                return;
            }

            List<PlayerControl> broadClientPool = PlayerControl.AllPlayerControls.ToArray()
                .Where(p => !p.AmOwner && p.PlayerId != Target.PlayerId)
                .Where(p => p.GetRoleManager().MyRole.Alignment == TargetAlignment)
                .ToList();
            List<PlayerControl> clientPool = broadClientPool
                .Where(p => p.Data.IsDead == player.IsDead)
                .ToList();
            if (clientPool.Count == 0) clientPool = broadClientPool;
            if (clientPool.Count == 0)
            {
                UsefulMethods.ShowTextToast(string.Format(LanguageProvider.Current.Lifeboat_Lawyer_Toast_ClientDcdTurnJest, 
                    Color, Jester.Settings.HeaderColor, LanguageProvider.Current.Lifeboat_Jester), 2.5f);
                    
                BecomeJester();
                AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Lawyer_BecomeJester).EndMessage();
                return;
            }

            PlayerControl newClient = clientPool.Shuffle().First();
            Target = newClient.Data;
            TargetAlignment = newClient.GetRoleManager().MyRole.Alignment;
            TargetSet = true;
            MessageWriter writer = AmongUsClient.Instance.StartRpc(Owner.NetId, (byte) CustomRpcCalls.Lawyer_SetClient, SendOption.Reliable);
            writer.Write(Target.PlayerId);
            writer.EndMessage();
                
            UsefulMethods.ShowTextToast(string.Format(LanguageProvider.Current.Lifeboat_Lawyer_Toast_ClientDcd, Color, Target.PlayerName), 2.5f);

            Arrow.gameObject.SetActive(!Target.IsDead);
        }
    }
        
    public void BecomeJester()
    {
        Jester jester = new();
        Owner.GetRoleManager().MyRole = jester;
        if (Target != null)
        {
            jester.PreviousRole = $"{PreviousRole}<color=#{Color.ToRGBAString()}>{RoleName}</color> ({Target?.PlayerName}) -> ";
            jester.LawyerRevealedPlayer = Target.PlayerId;
        }
        else
        {
            jester.PreviousRole = $"{PreviousRole}<color=#{Color.ToRGBAString()}>{RoleName}</color> -> ";
        }
        jester.Start();

        if (Owner.AmOwner)
        {
            UsefulMethods.ShowTextToast(string.Format(LanguageProvider.Current.Lifeboat_Lawyer_Toast_ClientDiedTurnJest, Color.ToRGBAString(), 
                jester.Color.ToRGBAString(), jester.RoleName), 2.5f);
                
            Owner.GetRoleManager().StartCoroutine(UsefulMethods.CoFlashScreen(new Color32(255, 117, 239, 77)));
        }
    }

    public void RegisterAdditionalWinners(TempWinData data)
    {
        if (!TargetSet) return;
            
        if (!Target.Disconnected)
        {
            foreach (byte player in data.WinnerIds)
            {
                if (player == Target.PlayerId)
                {
                    data.WinnerIds = data.WinnerIds.AddItem(Owner.PlayerId).ToArray();
                    break;
                }
            }
        }
    }
        
    public override List<PlayerControl> GetIntroTeam() => new() {PlayerControl.LocalPlayer, Target.Object};
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Lawyer_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString(), Target.PlayerName));
    }

    public override string GetImportantTaskText()
    {
        return base.GetImportantTaskText() + string.Format(LanguageProvider.Current.Lifeboat_Lawyer_TaskText, Color.ToRGBAString(), Target.PlayerName);
    }

    public string LawyerNameOverride(PlayerControl player, string currentName, bool inMeeting)
    {
        if (!player.IsThere() || Target == null) return currentName;
        if (player.PlayerId != Target.PlayerId) return currentName;
        if (PlayerControl.LocalPlayer.PlayerId != Owner.PlayerId && !RoleManager.SeesRolesAsGhost()) return currentName;

        BaseRole role = player!.GetRoleManager()?.MyRole;
        if (inMeeting && role != null && Target.IsDead && !RoleManager.SeesRolesAsGhost())
        {
            return $"<color=#{role.Color.ToRGBAString()}>{currentName}</color> <color=#{Color.ToRGBAString()}>[★]</color>\n" +
                   $"<size=70%><color=#{role.Color.ToRGBAString()}>{role.RoleName}";
        }

        return $"{currentName}{(currentName.Contains("]") ? "" : " ")}<color=#{Color.ToRGBAString()}>[★]</color>";
    }
        
    public override string GetGameSummaryDescription(bool hasWon)
    {
        return $"{(hasWon ? "<color=green>" : "")}{Owner.Data.PlayerName}:{(hasWon ? "</color>" : "")} " +
               $"{PreviousRole}" +
               $"<color=#{Color.ToRGBAString()}>{RoleName}</color> ({Target?.PlayerName})" +
               $"{(Owner.GetRoleManager().MyModifier is { } modifier ? " " + modifier.GetGameSummaryDescription() : "")}";
    }
        
    public override void Deserialize(MessageReader reader)
    {
        Target = GameData.Instance.GetPlayerById(reader.ReadByte());
        TargetSet = reader.ReadBoolean();
    }

    public override void Serialize(MessageWriter writer)
    {
        writer.Write(Target.PlayerId);
        writer.Write(TargetSet);
    }

    public override BaseRole CreateClone()
    {
        return new Lawyer
        {
            Target = Target,
            TargetSet = TargetSet,
        };
    }
        
    public static bool CheckLawyerProtection(PlayerControl killer, PlayerControl target, bool inMeeting, List<PlayerControl> showFlashTo = null)
    {
        showFlashTo ??= new List<PlayerControl> {killer};
            
        if (target.Data.IsDead) return false;
        if (target.GetRoleManager().MyRole is not Lawyer {HasShield: true} lawyer) return false;
        if (lawyer.Target.PlayerId != killer.PlayerId) return false;
            
        if (!inMeeting && killer.AmOwner) killer.GetRoleManager().MyRole.OnFailedNonMeetingKill();

        if (showFlashTo.Select(p => p.PlayerId).Contains(PlayerControl.LocalPlayer.PlayerId))
        {
            target.GetRoleManager().StartCoroutine(UsefulMethods.CoFlashScreen(new Color32(70, 235, 52, 77)));
        }

        lawyer.HasShield = false;
        return true;
    }
}