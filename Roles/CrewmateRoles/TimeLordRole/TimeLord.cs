using System.Collections.Generic;
using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Lifeboat.Attributes;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.GameOptions;
using Lifeboat.Roles.CrewmateRoles.TimeLordRole.Buttons;
using Submerged.Map;
using Submerged.Map.MonoBehaviours;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.TimeLordRole;

[OptionHeader(nameof(English.Lifeboat_TimeLord))]
public sealed class TimeLord : Crewmate
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_CrewmateRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_TimeLord), "TimeLord")]
    public static float TimeLordAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 91),
        HeaderColor = new Color32(0, 65, 130, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(TimeLordAmount, Alignment.Crewmate),
    };
        
    [ToggleOption(nameof(English.Lifeboat_TimeLord_GameOptions_CanUseVitals), "TimeLord Can Use Vitals", 10)] 
    public static bool CanUseVitals;
    public static bool CanUseVitals_GetVisible => GeneralOptions.ShouldShowMeaningless || PlayerControl.GameOptions.MapId switch
    {
        0 or 1 or 3 => false,
        2 or 4 => true,
        5 => SubmergedGameOptions.EnableVitals,
        _ => true,
    };
        
    [NumberOption(nameof(English.Lifeboat_TimeLord_GameOptions_Duration), "Rewind Duration", 9, 
        3, 10, 1, false, "{0}s")]
    public static float RewindDuration = 5;
        
    [NumberOption(nameof(English.Lifeboat_TimeLord_GameOptions_Cooldown), "Rewind Cooldown", 8, 
        12.5f, 60, 2.5f, false, "{0:0.0}s")] 
    public static float RewindCooldown = 25;

    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_TimeLord);
    public override Color32 Color => Settings.HeaderColor;

    public Dictionary<byte, LinkedList<PlayerPointInTime>> PointInTimesMap { get; set; }= new();
    public LinkedList<MapPointInTime> MapPointInTimes { get; set; } = new();
    public float LastTime { get; set; }

    public override void Start()
    {
        if (Owner.AmOwner) new RewindButton(this);
    }

    public override void Update()
    {
        if (!ShipStatus.Instance || MeetingHud.Instance || ExileController.Instance)
        {
            PointInTimesMap.Clear();
            MapPointInTimes.Clear();
            return;
        }
            
        LastTime += Time.deltaTime;
        if (RewindTime.RewindRoutine != null) return;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (!PointInTimesMap.TryGetValue(player.PlayerId, out LinkedList<PlayerPointInTime> playerPointInTimes)) playerPointInTimes = PointInTimesMap[player.PlayerId] = new LinkedList<PlayerPointInTime>();
            playerPointInTimes.AddLast(new PlayerPointInTime(player, LastTime));

            float currentTime = playerPointInTimes.First.Value.Time;
            if (currentTime < LastTime - RewindDuration) playerPointInTimes.RemoveFirst();
        }

        if (SubmarineStatus.Instance)
        {
            MapPointInTimes.AddLast(MapPointInTime.Create(LastTime));

            float currentMap = MapPointInTimes.First.Value.Time;
            if (currentMap < LastTime - RewindDuration) MapPointInTimes.RemoveFirst();
        }
    }
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_TimeLord_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }
}