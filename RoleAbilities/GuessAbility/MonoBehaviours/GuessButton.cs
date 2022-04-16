using System;
using Framework.Attributes;
using Framework.Utilities;
using Lifeboat.Roles;
using Lifeboat.Roles.ImpostorRoles;
using UnityEngine;
using UnityEngine.UI;

namespace Lifeboat.RoleAbilities.GuessAbility.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class GuessButton : MonoBehaviour
{
    public GuessButton(IntPtr ptr) : base(ptr) { }
        
    public Impostor Impostor;

    public SpriteRenderer Rend;
    public BoxCollider2D BoxCollider;
    public PassiveButton PassiveButton;
    public PlayerVoteArea Parent;
    public SpriteRenderer Highlight;

    public bool Disable;
    private void Start()
    {
        Rend = gameObject.AddComponent<SpriteRenderer>();
        Rend.sprite = ResourceManager.GetSprite("Guess");
            
        Transform highlightTransform = Instantiate(Parent.ConfirmButton.transform.GetChild(0).gameObject).transform;
        highlightTransform.parent = transform;
        highlightTransform.localScale = new Vector3(1.1f, 1.1f, 1);
        highlightTransform.localPosition = new Vector3(0, 0, 0.5f);
        Highlight = highlightTransform.GetComponent<SpriteRenderer>();
        Highlight.color = new Color32(18, 75, 136, 255);
            
        BoxCollider = gameObject.AddComponent<BoxCollider2D>();
        PassiveButton = gameObject.AddComponent<PassiveButton>();
        PassiveButton.Colliders = new[] {BoxCollider};
            
        PassiveButton.OnMouseOver = new Button.ButtonClickedEvent();
        PassiveButton.OnMouseOver.AddListener((Action) (() => Highlight.enabled = true));
            
        PassiveButton.OnMouseOut = new Button.ButtonClickedEvent();
        PassiveButton.OnMouseOut.AddListener((Action) (() => Highlight.enabled = false));
            
        PassiveButton.OnClick.AddListener((Action) Select);
    }

    private void Update()
    {
        if (PlayerControl.LocalPlayer.Data.IsDead || Parent.Parent.state == MeetingHud.VoteStates.Results || Impostor.GuessAbility.RemainingKills <= 0 || 
            GameData.Instance.GetPlayerById(Parent.TargetPlayerId) is null or {Disconnected: true} or {IsDead: true})
        {
            Destroy(gameObject);
        }
            
        PassiveButton.enabled = !Parent.Buttons.active && Parent.TargetPlayerId < 20 && !Disable;
        BoxCollider.enabled = !Parent.Buttons.active && Parent.TargetPlayerId < 20 && !Disable;
        Rend.enabled = !Parent.Buttons.active && Parent.TargetPlayerId < 20;

        Highlight.enabled &= Rend.enabled;
    }

    private void Select()
    {
        Impostor.GuessAbility.ShowGuessWindow(MeetingHud.Instance.transform, Parent.TargetPlayerId);
    }
}