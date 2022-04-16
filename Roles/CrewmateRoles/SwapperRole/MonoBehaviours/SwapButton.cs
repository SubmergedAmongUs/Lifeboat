using System;
using Framework.Attributes;
using Framework.Utilities;
using Lifeboat.RoleAbilities.SwapAbility;
using Lifeboat.RoleAbilities.SwapAbility.Interfaces;
using Lifeboat.RoleAbilities.SwapAbility.MonoBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace Lifeboat.Roles.CrewmateRoles.SwapperRole.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class SwapButton : BaseSwapButton, ISwapButton
{
    public SwapButton(IntPtr ptr) : base(ptr) { }

    public Sprite m_IdleSprite;
    public Sprite IdleSprite
    {
        get
        {
            if (m_IdleSprite) return m_IdleSprite;
            return m_IdleSprite = ResourceManager.GetSprite("SwapInactive");
        }
    }
        
    public Sprite m_ActiveSprite;
    public Sprite ActiveSprite
    {
        get
        {
            if (m_ActiveSprite) return m_ActiveSprite;
            return m_ActiveSprite = ResourceManager.GetSprite("SwapActive");
        }
    }

    public SwapAbility Swapper;

    public bool Selected => Swapper.Selected.Contains(this);
    public SpriteRenderer Rend;
    public BoxCollider2D BoxCollider;
    public PassiveButton PassiveButton;
    public PlayerVoteArea Parent { get; set; }
    public SpriteRenderer Highlight;

    public bool Disable;
    public void Start()
    {
        Rend = gameObject.AddComponent<SpriteRenderer>();
        Rend.sprite = IdleSprite;
            
        Transform highlightTransform = Instantiate(Parent.ConfirmButton.transform.GetChild(0).gameObject).transform;
        highlightTransform.parent = transform;
        highlightTransform.localScale = new Vector3(1.1f, 1.1f, 1);
        highlightTransform.localPosition = new Vector3(0, 0, 0.5f);
        Highlight = highlightTransform.GetComponent<SpriteRenderer>();
            
        BoxCollider = gameObject.AddComponent<BoxCollider2D>();
        PassiveButton = gameObject.AddComponent<PassiveButton>();
        PassiveButton.Colliders = new[] {BoxCollider};
            
        PassiveButton.OnMouseOver = new Button.ButtonClickedEvent();
        PassiveButton.OnMouseOver.AddListener((Action) (() => Highlight.enabled = true));
            
        PassiveButton.OnMouseOut = new Button.ButtonClickedEvent();
        PassiveButton.OnMouseOut.AddListener((Action) (() => Highlight.enabled = false));
            
        PassiveButton.OnClick.AddListener((Action) Select);
    }

    public void Update()
    {
        Rend.sprite = Selected ? ActiveSprite : IdleSprite;
        Highlight.color = Selected ? Color.green : new Color32(255, 174, 0, 255);

        PassiveButton.enabled = !Parent.Buttons.active && Parent.TargetPlayerId < 20 && !Disable;
        BoxCollider.enabled = !Parent.Buttons.active && Parent.TargetPlayerId < 20 && !Disable;
        Rend.enabled = !Parent.Buttons.active && Parent.TargetPlayerId < 20;

        Highlight.enabled &= Rend.enabled;

        if (GameData.Instance.GetPlayerById(Parent.TargetPlayerId) is not {IsDead: false, Disconnected: false})
        {
            if (Selected) Swapper.Selected.Remove(this);
            gameObject.SetActive(false);
        }
        if (PlayerControl.LocalPlayer.Data is not {IsDead: false}) gameObject.SetActive(false);
    }

    public void Select()
    {
        if (Selected)
        {
            Swapper.Selected.Remove(this);
        }
        else
        {
            if (Swapper.Selected.Count == 2) Swapper.Selected.RemoveAt(0);
            Swapper.Selected.Add(this);
        }
    }
}