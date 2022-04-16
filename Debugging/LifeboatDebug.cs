using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Debugging;
using Framework.Extensions;
using Lifeboat.CustomAppearance;
using Lifeboat.Extensions;
using Lifeboat.RoleModifiers.Modifiers.LoversModifier;
using Lifeboat.RoleModifiers.Modifiers.LoversModifier.MonoBehaviours;
using Lifeboat.Roles;
using Lifeboat.Roles.NeutralRoles.JesterRole;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Lifeboat.Debugging;

public sealed class LifeboatDebug
{
    public static LifeboatDebug Instance;

    public LifeboatDebug()
    {
#if DEBUG
        GUIWindow.DebugWindow.GUIWindowTabs.Add(new GUIWindowTab("Lifeboat", BuildLifeboatWindow, () => PlayerControl.LocalPlayer));
#else
            GUIWindow.DebugWindow.GUIWindowTabs.Add(new GUIWindowTab("Debugging", BuildIntroWindow, () => true));
#endif
        GUIWindow.DebugWindow.GUIWindowTabs.Add(new GUIWindowTab("Roles", BuildForceRoleWindow, () => PlayerControl.LocalPlayer));
    }

    public bool RunStartOnSetRole { get; set; } = true;
    public bool MakeDummiesVoteForMe { get; set; }

    public bool DisableEndGame { get; set; }

    public IEnumerator TestLoverMessage()
    {
        foreach (Color32 color32 in Palette.PlayerColors)
        {
            GameObject bubbleObj = GameObject.Instantiate(Lovers.TextBubblePrefab, HudManager.Instance.transform, true);
            bubbleObj.layer = 5;
            bubbleObj.transform.GetChild(0).gameObject.layer = 5;
            bubbleObj.transform.localScale = new Vector3(0.0666f, 0.0666f, 1);

            TextMessage message = bubbleObj.AddComponent<TextMessage>();
            message.Renderer = bubbleObj.GetComponent<SpriteRenderer>();
            message.Tail = true;
            message.PlainSprite = bubbleObj.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite;
            message.TailSprite = message.Renderer.sprite;
            message.TextMeshPro = bubbleObj.GetComponentInChildren<TextMeshPro>();
            message.Message = "Hello there!";

            message.Renderer.flipX = true;
            Color color = message.Renderer.color = color32;
            message.TextMeshPro.color = 1.51f - color.r - color.g - color.b > 0 ? Color.white : Color.black;
            message.UpdateText();

            AspectPosition aspectPosition = bubbleObj.AddComponent<AspectPosition>();
            aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;

            aspectPosition.DistanceFromEdge = new Vector3(1.1f + 1.45f, 0.19625f + 0.075f * message.Lines, -10f);
            aspectPosition.AdjustPosition();

            message.Show();

            yield return new WaitForSeconds(0.25f);
        }
    }

    public void BuildLifeboatWindow()
    {
        DisableEndGame = GUILayout.Toggle(DisableEndGame, "Disable End Game", GUIWindow.EmptyOptions);
        RunStartOnSetRole = GUILayout.Toggle(RunStartOnSetRole, "Run Start On Role Set", GUIWindow.EmptyOptions);
        MakeDummiesVoteForMe = GUILayout.Toggle(MakeDummiesVoteForMe, "Make Dummies Vote For Me", GUIWindow.EmptyOptions);

        if (GUILayout.Button("Show Intro", GUIWindow.EmptyOptions))
        {
            HudManager.Instance.FullScreen.enabled = true;
            HudManager.Instance.FullScreen.color = Color.black;
            HudManager.Instance.StartCoroutine(HudManager.Instance.CoShowIntro(new Il2CppSystem.Collections.Generic.List<PlayerControl>()));
        }
            
        if (GUILayout.Button("Test Lover", GUIWindow.EmptyOptions))
        {
            HudManager.Instance.StartCoroutine(TestLoverMessage());
        }
            
        if (GUILayout.Button("Test Appearance Manager", GUIWindow.EmptyOptions))
        {
            AppearanceModification modification = new()
            {
                Data = new AppearanceData
                {
                    Name = "POGGERS WORKING",
                    ColorId = Palette.PlayerColors.RandomIdx(),
                    HatId = (uint) Random.Range(0, HatManager.Instance.AllHats.Count),
                    SkinId = 5U, //(uint) Random.Range(0, HatManager.Instance.AllSkins.Count),
                    PetId = 2U, //(uint) Random.Range(0, HatManager.Instance.AllPets.Count),
                    Alpha = 0.26f
                },
                    
                ModificationMask = AppearanceModification.Overrides.Name |
                                   AppearanceModification.Overrides.ColorId |
                                   AppearanceModification.Overrides.HatId |
                                   AppearanceModification.Overrides.SkinId |
                                   AppearanceModification.Overrides.PetId |
                                   AppearanceModification.Overrides.Alpha,
                Priority = 1,
                Timer = 100f
            };
            
            PlayerControl.LocalPlayer.GetAppearanceManager().Modifications.Add(modification);
        }

        if (GUILayout.Button("End Game", GUIWindow.EmptyOptions))
        {
            Jester jester = new();
            PlayerControl.LocalPlayer.GetComponent<RoleManager>().MyRole = jester;
            jester.JesterWin();
        }

        GUILayout.Label("Set My Role", GUIWindow.EmptyOptions);
        foreach ((string roleStringID, Type roleType) in BaseRole.StringIDToRoleType.OrderBy(t => t.Key))
        {
            if (GUILayout.Button(TrimLifeboatPrefix(roleStringID), GUIWindow.EmptyOptions))
            {
                BaseRole myRole = (BaseRole) Activator.CreateInstance(roleType);
                PlayerControl.LocalPlayer.GetComponent<RoleManager>().MyRole = myRole;
                myRole.Owner = PlayerControl.LocalPlayer;
                if (RunStartOnSetRole) myRole.Start();
            }
        }
            
        GUILayout.Label("Set My Modifier", GUIWindow.EmptyOptions);
        if (GUILayout.Button("None", GUIWindow.EmptyOptions))
        {
            PlayerControl.LocalPlayer.GetComponent<RoleManager>().MyModifier = null;
        }
        if (GUILayout.Button("Lovers", GUIWindow.EmptyOptions))
        {
            Lovers modifier = new(PlayerControl.AllPlayerControls.ToArray().First(p => p.PlayerId != PlayerControl.LocalPlayer.PlayerId).PlayerId);
            PlayerControl.LocalPlayer.GetComponent<RoleManager>().MyModifier = modifier;
            modifier.Owner = PlayerControl.LocalPlayer;
            if (RunStartOnSetRole) modifier.Start();
        }
    }

    public bool ForceRolesEnabled { get; set; }
    public Dictionary<byte, int> RoleData { get; set; } = new();

    public void BuildForceRoleWindow()
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            GUILayout.Label("You are not the host!", GUIWindow.EmptyOptions);
            return;
        }

        List<Type> Roles = BaseRole.RoleTypeToStringID.OrderBy(t => t.Value).Select(s => s.Key).ToList();

        ForceRolesEnabled = GUILayout.Toggle(ForceRolesEnabled, $"Managed Roles {(ForceRolesEnabled ? "Enabled" : "Disabled")}", new GUIStyle(GUI.skin.button), GUIWindow.EmptyOptions);
        if (!ForceRolesEnabled) return;
            
        GUILayout.Label("Left Click -> Forward", GUIWindow.EmptyOptions);
        GUILayout.Label("Right Click -> Backward", GUIWindow.EmptyOptions);

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            GUILayout.Label(player.Data.PlayerName, GUIWindow.EmptyOptions);

            if (!RoleData.ContainsKey(player.PlayerId)) RoleData[player.PlayerId] = 0;
            if (GUILayout.Button($"CurrentRole: {TrimLifeboatPrefix(BaseRole.RoleTypeToStringID[Roles[RoleData[player.PlayerId]]])}", GUIWindow.EmptyOptions))
            {
                if (Event.current.button == 1)
                {
                    RoleData[player.PlayerId] = (RoleData[player.PlayerId] - 1 + Roles.Count) % Roles.Count;
                }
                else
                {
                    RoleData[player.PlayerId] = (RoleData[player.PlayerId] + 1) % Roles.Count;
                }
            }
        }
    }

    public void BuildIntroWindow()
    {
        GUILayout.Label("This is a development tool, you shouldn't be here.", GUIWindow.EmptyOptions);
        if (GUILayout.Button("Press F1+F2 To Close", GUIWindow.EmptyOptions)) GUIWindow.DebugWindow.Enabled = false;
    }

    public string TrimLifeboatPrefix(string str)
    {
        return str.StartsWith("Lifeboat_") ? str[9..] : str;
    }
}