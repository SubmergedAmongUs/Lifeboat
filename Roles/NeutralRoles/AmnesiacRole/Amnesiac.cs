using APIs.CustomOptions.Attributes;
using APIs.CustomOptions.Attributes.CustomOptionAttributes;
using APIs.CustomOptions.CustomOptions;
using Essentials.PluginLoader.Extensions;
using Hazel;
using Mods.Lifeboat.Attributes;
using Mods.Lifeboat.Enums;
using Mods.Lifeboat.Extensions;
using Mods.Lifeboat.MonoBehaviours;
using UnityEngine;

namespace Mods.Lifeboat.Roles.NeutralRoles.AmnesiacRole
{
    [OptionHeader("Amnesiac")]
    public sealed class Amnesiac : BaseRole
    {
        #region Options
        
        [OptionHeader("Neutral", int.MinValue)] [RoleAmount] [NumberOption("Amnesiac")] public static float AmnesiacAmount = 0;

        public static OptionHeaderSettings Settings = new()
        {
            HeaderColor = new Color32(255, 242, 0, 255),
            DefaultOpenInConsole = false,
            GroupVisible = () => AmnesiacAmount > 0
        };

        [StringOption("Announce Amnesiac's New...", "Role", "Alignment (Except Glitch)", "Alignment", "Don't Announce")]
        public static int AnnounceSetting = 1;

        [ToggleOption("Allow Amnesiac To Reuse One-Time Abilities")]
        public static bool ReuseAbilities = true;
        
        #endregion
        
        public override string Name => "Amnesiac";
        public override Color32 Color => new(255, 242, 0, 255);
        public override Alignment Alignment => Alignment.Neutral;

        public override void Start()
        {
            
        }

        public override void SetIntroAppearance(IntroCutscene introCutscene) => this.DefaultIntroCutscene(introCutscene, $"Try to remember your role");

        public bool Remember(byte playerId)
        {
            GameData.PlayerInfo info = GameData.Instance.GetPlayerById(playerId);
            if (info == null || info.Disconnected || !info.IsDead) return false;

            PlayerControl control = info.Object;
            if (!control) return false;

            RoleManager roleManager = control.GetRoleManager();
            if (!roleManager) return false;

            MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte) CustomRpcCalls.Amnesiac_Remember, SendOption.Reliable);
            writer.Write(roleManager.MyRole.GetType().FullName);
            roleManager.MyRole.Serialize(writer);
            writer.EndMessage();

            BaseRole newRole = roleManager.MyRole.CreateClone();
            if (newRole == null) return false;
            
            Owner.GetRoleManager().MyRole = newRole;
            newRole.Start();
            newRole.PreviousRole = $"{PreviousRole}<color=#{Color.ToRGBAString()}>{Name}</color> -> ";

            return true;
        }
    }
}