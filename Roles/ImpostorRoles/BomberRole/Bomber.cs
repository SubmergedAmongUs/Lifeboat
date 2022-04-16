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
using Lifeboat.Attributes;
using Lifeboat.CustomAppearance;
using Lifeboat.Enums;
using Lifeboat.Events;
using Lifeboat.Extensions;
using Lifeboat.Patches;
using Lifeboat.Roles.CrewmateRoles.MedicRole;
using Lifeboat.Roles.ImpostorRoles.BomberRole.Buttons;
using Lifeboat.Roles.NeutralRoles.LawyerRole;
using UnityEngine;

namespace Lifeboat.Roles.ImpostorRoles.BomberRole;

[OptionHeader(nameof(English.Lifeboat_Bomber))]
public sealed class Bomber : Impostor
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_ImpostorRoles), int.MinValue)] [RoleAmount]
    [NumberOption(nameof(English.Lifeboat_Bomber), "Bomber")]
    public static float BomberAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 59),
        HeaderColor = new Color32(242, 149, 0, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(BomberAmount, Alignment.Impostor),
    };

    [NumberOption(nameof(English.Lifeboat_Bomber_GameOptions_RevealDelay), "Bomber_RevealDelay", 10, 
        3, 20, 1f, false, "{0:0}s")]
    public static float RevealDelay = 5;

    [NumberOption(nameof(English.Lifeboat_Bomber_GameOptions_FuseDuration), "Bomber_FuseDuration", 9, 
        10, 240, 5f, false, "{0:0}s")]
    public static float FuseDuration = 30;
        
    #endregion

    public override string RoleStringID => nameof(English.Lifeboat_Bomber);
    public override Color32 Color => Settings.HeaderColor;

    public PlantBombButton Button { get; set; }
    public byte BombTarget { get; set; } = byte.MaxValue;

    public override void Start()
    {
        base.Start();
            
        if (!ResourceManager.SpriteCache.ContainsKey("Bomb")) ResourceManager.CacheSprite("Bomb", 250);
        
        if (Owner.AmOwner)
        {
            Button = new PlantBombButton(this);
                
            PlayerControlEvents.OnPlayerMurder += HandlePlayerMurder;
            PlayerControlEvents.OnPlayerDisconnect += HandlePlayerDisconnect;
        }
            
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            RoleManager manager = playerControl!?.GetRoleManager();
            if (manager) manager.NameOverrides.Add(new NameOverride(BomberNameOverride, 13));
        }
    }

    public override void OnDestroy()
    {
        if (BombTarget != byte.MaxValue)
        {
            PlayerControl target = GameData.Instance.GetPlayerById(BombTarget).Object;
            Explode(target, target);
        }
            
        base.OnDestroy();
        BombTarget = byte.MaxValue;
            
        PlayerControlEvents.OnPlayerMurder -= HandlePlayerMurder;
        PlayerControlEvents.OnPlayerDisconnect -= HandlePlayerDisconnect;
    }
        
    public override void OnFailedNonMeetingKill()
    {
        base.OnFailedNonMeetingKill();
        Button.CurrentTime = PlayerControl.GameOptions.KillCooldown;
    }
        
    #region Prevent body reports for local player

    public List<byte> CannotReport { get; } = new();

    public override void Update()
    {
        base.Update();
            
        Vector2 myPosition = PlayerControl.LocalPlayer.GetTruePosition();

        float maxDistance = PlayerControl.LocalPlayer.MaxReportDistance;
        Collider2D closestHit = null;

        foreach (Collider2D body in Physics2D.OverlapCircleAll(myPosition, PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
        {
            if (!body.CompareTag("DeadBody")) continue;
            float distance = Vector2.Distance(body.transform.position, myPosition);
                
            if (distance < maxDistance)
            {
                DeadBody component = body.GetComponent<DeadBody>();
                if (!CannotReport.Contains(component.ParentId) && component.enabled &&
                    !PhysicsHelpers.AnythingBetween(myPosition, component.TruePosition, Constants.ShipAndObjectsMask, false))
                {
                    maxDistance = distance;
                    closestHit = body;
                }
            }
        }

        HudManager.Instance.ReportButton.SetActive(closestHit);
    }
        
    #endregion

    public void HandlePlayerMurder(PlayerControl killer, PlayerControl target)
    {
        if (Owner.AmOwner && killer.AmOwner && target && !Owner.Data.IsDead) Button.CurrentTime = Button.Cooldown;
        if (target.PlayerId == BombTarget)
        {
            BombTarget = byte.MaxValue;

            Owner.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
            Button.CurrentTime = PlayerControl.GameOptions.KillCooldown;   
        }
    }

    public void HandlePlayerDisconnect(GameData.PlayerInfo player)
    {
        if (player.PlayerId == BombTarget)
        {
            BombTarget = byte.MaxValue;

            Owner.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
            Button.CurrentTime = PlayerControl.GameOptions.KillCooldown;   
        }
    }

    public string BomberNameOverride(PlayerControl player, string currentName, bool inMeeting)
    {
        if (!player.IsThere() || player.Data.IsDead) return currentName;
        if (player.PlayerId != BombTarget) return currentName;
        if (PlayerControl.LocalPlayer.GetRoleManager().MyRole.Alignment != Alignment.Impostor && !RoleManager.SeesRolesAsGhost()) return currentName;

        return $"{currentName}{(currentName.Contains("]") ? "" : " ")}<color=#{Settings.HeaderColor.ToRGBAString()}>[※]</color>";
    }

    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Bomber_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public void PlantBomb(PlayerControl target)
    {
        Owner.GetRoleManager().StartCoroutine(CoPlantBomb(target));
        BombTarget = target.PlayerId;
    }

    public IEnumerator CoPlantBomb(PlayerControl target)
    {
        for (float t = 0; t < RevealDelay; t += Time.deltaTime)
        {
            yield return null;

            if (MeetingHud.Instance || target.Data.IsDead || target.Data.Disconnected)
            {
                BombTarget = byte.MaxValue;
                    
                if (Owner.AmOwner)
                {
                    PlayerControl.LocalPlayer.GetRoleManager().StopCoroutine(Button.EffectCoroutine);
                        
                    Button.KillButtonManager.TimerText.color = UnityEngine.Color.white;
                    Button.CurrentTime = PlayerControl.GameOptions.KillCooldown;
                    PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
                }
                    
                yield break;
            }
        }
            
        if (target.AmOwner) new ThrowBombButton(this);
    }

    public void Explode(PlayerControl killer, PlayerControl target)
    {
        List<PlayerControl> showFlashTo = new() {killer, Owner};
        CrewmateRoles.MedicRole.Patches.PlayerControl_MurderPlayer_Patch.ShowFlashTo = showFlashTo;
        NeutralRoles.LawyerRole.Patches.PlayerControl_MurderPlayer_Patch.ShowFlashTo = showFlashTo;
            
        killer.MurderPlayer(target);
        target.GetRoleManager().KilledBy = Owner.PlayerId;
        BombTarget = byte.MaxValue;
            
        if (Owner.AmOwner && Button != null)
        {
            Owner.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
            Button.CurrentTime = PlayerControl.GameOptions.KillCooldown;
        }
    }
        
    public void ExplodeAtMeeting(PlayerControl target)
    {
        PlayerVoteArea voteArea = MeetingHud.Instance.playerStates.First(x => x.TargetPlayerId == target.PlayerId);

        ShipStatus_CheckEndCriteria_Patch.Timeout = 50;

        List<PlayerControl> showFlashTo = new() {Owner, target};
        if (Medic.CheckMedicProtection(Owner, target, true, showFlashTo)) return;
        if (Lawyer.CheckLawyerProtection(Owner, target, true, showFlashTo)) return;
            
        PlayerControlEvents.OnPlayerCustomMurder?.Invoke(Owner, target);

        if (target.AmOwner)
        {
            HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
            target.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
            target.RpcSetScanner(false);

            SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);
            HudManager.Instance.KillOverlay.ShowKillAnimation(target.Data, target.Data);
        }

        BombTarget = byte.MaxValue;
        target.Die(DeathReason.Kill);
        target.GetRoleManager().KilledBy = Owner.PlayerId;
            
        if (target.AmOwner)
        {
            MeetingHud.Instance.SetForegroundForDead();
        }

        if (!voteArea.IsThere()) return;
            
        voteArea.AmDead = true;
        voteArea.Overlay.gameObject.SetActive(true);
        voteArea.Overlay.color = UnityEngine.Color.white;
        voteArea.XMark.gameObject.SetActive(true);
        voteArea.XMark.transform.localScale = Vector3.one;
            
        MeetingHud.Instance.SortButtons();
    }
}