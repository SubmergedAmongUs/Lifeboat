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
using Lifeboat.Roles.ImpostorRoles.MinerRole.Buttons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Lifeboat.Roles.ImpostorRoles.MinerRole;

[OptionHeader(nameof(English.Lifeboat_Miner))]
public sealed class Miner : Impostor
{
    #region Option

    [OptionHeader(nameof(English.Lifeboat_GameOptions_ImpostorRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Miner), "Miner")] public static float MinerAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 55),
        HeaderColor = Palette.ImpostorRed,
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(MinerAmount, Alignment.Impostor),
    };
        
    [NumberOption(nameof(English.Lifeboat_Miner_GameOptions_Cooldown), "Mining Cooldown", 10,
        5, 60, 2.5f, false, "{0:0.0}s")] 
    public static float MineCooldown = 25;

    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Miner);
        
    public Vector2 VentScale => Vector2.Scale(ShipStatus.Instance.AllVents[5].GetComponent<BoxCollider2D>().size, ShipStatus.Instance.AllVents[0].transform.localScale) * 0.75f;
    public bool PlacedAVent { get; set; }
        
    public override void Start()
    {
        base.Start();
        if (Owner.AmOwner) new MineButton(this);
    }
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Miner_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }
        
    public void RpcPlaceVent(Vector2 position)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Miner_PlaceVent, SendOption.Reliable);
        writer.Write(position);
        writer.EndMessage();
            
        Place(position);
    }

    public void Place(Vector2 position)
    {
        GameObject ventGameObject = Object.Instantiate(ShipStatus.Instance.AllVents[5].gameObject);
        Vent newVent = ventGameObject.GetComponent<Vent>();
        newVent.transform.localScale = ShipStatus.Instance.AllVents[5].transform.lossyScale;
        newVent.Center = null;
        newVent.Right = null;
        newVent.Left = !PlacedAVent ? null : ShipStatus.Instance.AllVents.Last();
        newVent.Id = ShipStatus.Instance.AllVents.Count;
        newVent.spreadAmount = 10;

        Vector3 adjPos = new(position.x, position.y, position.y / 1000f + 0.001f);
            
        newVent.transform.position = adjPos;
            
        if (PlacedAVent)
        {
            ShipStatus.Instance.AllVents.Last().Right = newVent;
        }

        List<Vent> newVents = ShipStatus.Instance.AllVents.ToList();
        newVents.Add(newVent);
        ShipStatus.Instance.AllVents = newVents.ToArray();
        PlacedAVent = true;
    }
}