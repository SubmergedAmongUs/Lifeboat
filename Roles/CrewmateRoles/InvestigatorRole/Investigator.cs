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
using Lifeboat.Enums;
using Lifeboat.Extensions;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.InvestigatorRole;

[OptionHeader(nameof(English.Lifeboat_Investigator))]
public sealed class Investigator : Crewmate
{
    public const float MAX_DELTA = 0.4f;

    #region Options
        
    [OptionHeader(nameof(English.Lifeboat_GameOptions_CrewmateRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Investigator), "Investigator")] 
    public static float InvestigatorAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 98),
        HeaderColor = new Color32(52, 77, 201, 255),
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(InvestigatorAmount, Alignment.Crewmate),
    };

    [NumberOption(nameof(English.Lifeboat_Investigator_GameOptions_FootprintDuration), "Footprint Duration", 10,
        3, 20, 1, false, "{0}s")]
    public static float FootprintDuration = 5;
        
    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Investigator);
    public override Color32 Color => Settings.HeaderColor;

    public Sprite Footprint
    {
        get
        {
            if (m_Footprint) return m_Footprint;
            return m_Footprint = ResourceManager.GetSprite("Footprint");
        }
    }
    public List<SpriteRenderer> DeadPool { get; set; } = new();
    public List<(float placementTime, SpriteRenderer footPrint)> AlivePool { get; set; } = new();
    public Dictionary<byte, Vector2> PlayerPositions { get; set; } = new();

    private Sprite m_Footprint;

    public override void Start()
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            PlayerPositions[player.PlayerId] = player.GetTruePosition();
        }
    }
        
    public override void Update()
    {
        if (!Owner.AmOwner) return;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (!player || player.AmOwner || player.Data.IsDead) continue;
            Vector2 newPos = player.GetTruePosition();

            if (!PlayerPositions.ContainsKey(player.PlayerId)) PlayerPositions[player.PlayerId] = newPos;

            Vector2 oldPos = PlayerPositions[player.PlayerId];
            float distance = Vector2.Distance(oldPos, newPos);

            if (distance >= MAX_DELTA)
            {
                SpriteRenderer print = GetFootprint(player);
                AlivePool.Add((0, print));
                PlayerPositions[player.PlayerId] = newPos;
                print.transform.position = new Vector3(newPos.x, newPos.y, (newPos.y + 1f) / 1000f);

                Vector2 delta = newPos - oldPos;
                float angle = Mathf.Atan2(delta.y, delta.x) * MathConsts.Rad2Deg;

                print.transform.localEulerAngles = new Vector3(0, 0, angle + 90);
            }
        }

        for (int i = 0; i < AlivePool.Count; i++)
        {
            (float placementTime, SpriteRenderer footPrint) data = AlivePool[i];

            data.placementTime += Time.deltaTime;

            if (data.placementTime > FootprintDuration)
            {
                AlivePool.RemoveAt(i);
                data.footPrint.enabled = false;
                DeadPool.Add(data.footPrint);
                i--;
            }
            else
            {
                AlivePool[i] = data;
            }
        }
    }
        
    public override void OnDestroy()
    {
        base.OnDestroy();
            
        if (AlivePool != null)
        {
            foreach ((float placementTime, SpriteRenderer footPrint) item in AlivePool)
            {
                try
                {
                    item.footPrint!?.gameObject.Destroy();
                }
                catch
                {
                    // Ignore
                }
            }
        }

        if (DeadPool != null)
        {
            foreach (SpriteRenderer renderer in DeadPool)
            {
                try
                {
                    renderer!?.gameObject.Destroy();
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }

    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Investigator_IntroText, Color.ToRGBAString(), 
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }
        
    public SpriteRenderer GetFootprint(PlayerControl player)
    {
        SpriteRenderer footprint;
        if (DeadPool.Any())
        {
            footprint = DeadPool.RemoveAndGet(0);
            footprint.enabled = true;
        }
        else
        {
            footprint = new GameObject().AddComponent<SpriteRenderer>();
            footprint.gameObject.layer = 8;
            footprint.transform.localScale = Vector3.one * 0.5f;
            footprint.material = new Material(player.myRend.material);
            footprint.sprite = Footprint;
            player.SetPlayerMaterialColors(footprint);
        }

        footprint.color = new Color(1, 1, 1, player.Data.IsDead ? 0 : 0.5f);
        return footprint;
    }
}