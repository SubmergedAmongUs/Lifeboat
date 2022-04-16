using System.Linq;
using Framework.CustomOptions.Attributes;
using Framework.CustomOptions.Attributes.CustomOptionAttributes;
using Framework.CustomOptions.CustomOptions;
using Framework.Extensions;
using Framework.Localization;
using Framework.Localization.Languages;
using Framework.Utilities;
using Hazel;
using Lifeboat.Attributes;
using Lifeboat.Enums;
using Lifeboat.Extensions;
using Lifeboat.Roles.CrewmateRoles.AltruistRole;
using Lifeboat.Roles.ImpostorRoles.UndertakerRole.Buttons;
using Submerged.Map.MonoBehaviours;
using Object = UnityEngine.Object;

namespace Lifeboat.Roles.ImpostorRoles.UndertakerRole;

[OptionHeader(nameof(English.Lifeboat_Undertaker))]
public sealed class Undertaker : Impostor
{
    #region Options

    [OptionHeader(nameof(English.Lifeboat_GameOptions_ImpostorRoles), int.MinValue)] [RoleAmount] 
    [NumberOption(nameof(English.Lifeboat_Undertaker), "Undertaker")] 
    public static float UndertakerAmount = 0;

    public static OptionHeaderSettings Settings = new()
    {
        PriorityOverride = (true, 51),
        HeaderColor = Palette.ImpostorRed,
        DefaultOpenInConsole = false,
        GroupVisible = () => RoleManager.CouldAppear(UndertakerAmount, Alignment.Impostor),
    };

    [NumberOption(nameof(English.Lifeboat_Undertaker_GameOptions_Cooldown), "Undertaker Drag Cooldown", 10,
        2.5f, 30, 2.5f, false, "{0:0.0}s")]
    public static float DragCooldown = 10;
        
    #endregion
        
    public override string RoleStringID => nameof(English.Lifeboat_Undertaker);

    public override bool CanUseVents()
    {
        return !Corpse.IsThere();
    }

    public CustomDeadBody Corpse { get; set; }
    public DragDropButton DragDropButton { get; set; }
        
    public override void Start()
    {
        base.Start();
        if (Owner.AmOwner)
        {
            ResourceManager.CacheSprite("DragDrop", 250);
            DragDropButton = new DragDropButton(this, 0);
        }
            
        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
        {
            if (playerControl.IsThere() && playerControl.Data is {Disconnected: false} && playerControl.GetRoleManager()!?.MyRole is Altruist altruist)
            {
                if (!altruist.Undertakers.Contains(this)) altruist.Undertakers.Add(this);
            }
        }
    }

    public override void Update()
    {
        base.Update();
            
        if (!Corpse.IsThere()) return;
            
        if (Owner.Data.IsDead || Owner.Data.Disconnected || MeetingHud.Instance.IsThere())
        {
            Corpse!.Drop();
            Corpse = null;
        }
    }
        
    public override void SetIntroAppearance(IntroCutscene introCutscene)
    {
        this.DefaultIntroCutscene(introCutscene, string.Format(LanguageProvider.Current.Lifeboat_Undertaker_IntroText, Color.ToRGBAString(),
            Palette.CrewmateBlue.ToRGBAString(), Palette.ImpostorRed.ToRGBAString()));
    }

    public void RpcDragBody(DeadBody body)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Undertaker_Drag, SendOption.Reliable);
        writer.Write(body.ParentId);
        writer.EndMessage();
            
        Drag(body);
    }

    public void Drag(byte targetBodyId)
    {
        Drag(Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == targetBodyId));
    }
        
    public void Drag(DeadBody body)
    {
        if (!body) return;
        Corpse = body.GetComponent<CustomDeadBody>();
        Corpse.Drag(Owner);
    }

    public void RpcDropBody(DeadBody body)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Undertaker_Drop, SendOption.Reliable);
        writer.Write(body.ParentId);
        writer.EndMessage();
            
        Drop(body);
    }

    public void Drop(byte targetBodyId)
    {
        Drop(Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == targetBodyId));
    }

    public void Drop(DeadBody body)
    {
        Corpse = null;
        if (!body) return;
        CustomDeadBody customBody = body.GetComponent<CustomDeadBody>();
        customBody.Drop();
    }
}