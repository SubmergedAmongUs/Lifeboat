using System;
using Essentials.Common.Utilities;
using Essentials.PluginPreloader.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Mods.Lifeboat.Roles.NeutralRoles.AmnesiacRole.MonoBehaviours
{
    [RegisterInIl2Cpp]
    public class RememberButton : MonoBehaviour
    {
        public RememberButton(IntPtr ptr) : base(ptr) { }
        
        public Amnesiac Amnesiac;

        public SpriteRenderer Rend;
        public BoxCollider2D BoxCollider;
        public PassiveButton PassiveButton;
        public PlayerVoteArea Parent;
        public SpriteRenderer Highlight;

        private void Start()
        {
            Rend = gameObject.AddComponent<SpriteRenderer>();
            Rend.sprite = ResourceManager.GetSprite("AmnesiacRemember.png");
            
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
            if (PlayerControl.LocalPlayer.Data.IsDead || !GameData.Instance.GetPlayerById(Parent.TargetPlayerId).IsDead || 
                GameData.Instance.GetPlayerById(Parent.TargetPlayerId).Disconnected || Parent.Parent.state == MeetingHud.VoteStates.Results) Destroy(gameObject);
            
            PassiveButton.enabled = !Parent.Buttons.active && Parent.TargetPlayerId < 20;
            BoxCollider.enabled = !Parent.Buttons.active && Parent.TargetPlayerId < 20;
            Rend.enabled = !Parent.Buttons.active && Parent.TargetPlayerId < 20;

            Highlight.enabled &= Rend.enabled;
        }

        private void Select()
        {
            if (!Amnesiac.Remember(Parent.TargetPlayerId)) Destroy(gameObject);
        }
    }
}