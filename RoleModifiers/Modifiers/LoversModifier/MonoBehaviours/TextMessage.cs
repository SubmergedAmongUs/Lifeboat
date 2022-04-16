using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Attributes;
using Framework.Extensions;
using Framework.Utilities;
using TMPro;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace Lifeboat.RoleModifiers.Modifiers.LoversModifier.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class TextMessage : MonoBehaviour
{
    public TextMessage(IntPtr ptr) : base(ptr) { }

    public static List<TextMessage> Messages = new();
        
    public bool Tail;
    public string Message;
    public SpriteRenderer Renderer;
    public TextMeshPro TextMeshPro;
    public Sprite PlainSprite;
    public Sprite TailSprite;
    public int Lines;

    public Vector3 OrigPos;
    public Vector3 NewPos;

    private void Awake()
    {
        Messages.Add(this);
    }

    private void OnDestroy()
    {
        Messages.Remove(this);
    }

    public void UpdateText()
    {
        TextMeshPro.text = Message;
        TextMeshPro.ForceMeshUpdate();
        Lines = TextMeshPro.textInfo.lineCount;

        TextMeshPro.rectTransform.sizeDelta = new Vector2(27.2f, 0.3f + 2.33f * Lines);
        Renderer.sprite = !Tail ? PlainSprite : TailSprite; 
        Renderer.size = !Tail ? new Vector2(30f, 2.275f + 2.33f * Lines) : new Vector2(32.44f, 4.67f + Lines * 2.30667f);
        // 32.44 - Width
        // 11.59 - 3
        // 18.51 - 6
    }
        
    public void Show()
    {
        this.StartCoroutine(CoShow());
    }

    [HideFromIl2Cpp]
    public IEnumerator CoShow()
    {
        if (HudManager.Instance.Chat.IsOpen && HudManager.Instance.Chat.name == "LoversChatController")
        {
            Destroy(gameObject);
            yield break;
        }

        float duration = 0.15f;
        Vector3 originalScale = transform.localScale;
        Vector3 zero = new(0, 0, originalScale.z);
        transform.localScale = zero;
            
        float moveDur = 0.15f;
        float delta = 0.075f * 2 * (Lines) + 0.205f;

        Dictionary<TextMessage, (Vector3 orig, Vector3 newVec)> vecs = new(Il2CppEqualityComparer<TextMessage>.Instance);
            
        foreach (TextMessage textMessage in Messages.Where(t => t != this))
        {
            Vector3 pos = textMessage.transform.localPosition;
            Vector3 newPos = new(pos.x, pos.y + delta, pos.z);
                
            vecs[textMessage] = (pos, newPos);
            textMessage.Tail = false;
            textMessage.UpdateText();
        }

        for (float t = 0; t < moveDur; t += Time.deltaTime)
        {
            foreach (TextMessage textMessage in Messages.Where(m => m != this))
            {
                if (!vecs.ContainsKey(textMessage))
                {
                    Vector3 pos = textMessage.transform.localPosition;
                    Vector3 newPos = new(pos.x, pos.y + delta, pos.z);
                
                    vecs[textMessage] = (pos, newPos);
                }
                    
                (Vector3 orig, Vector3 newVec) data = vecs[textMessage];
                textMessage.transform.localPosition = Vector3.Lerp(data.orig, data.newVec, t / moveDur);
            }

            yield return null;
        }
            
        foreach (TextMessage textMessage in Messages.Where(t => t != this))
        {
            if (!vecs.ContainsKey(textMessage))
            {
                Vector3 pos = textMessage.transform.localPosition;
                Vector3 newPos = new(pos.x, pos.y + delta, pos.z);
                
                vecs[textMessage] = (pos, newPos);
            }
                
            (Vector3 orig, Vector3 newVec) data = vecs[textMessage];
            textMessage.transform.localPosition = data.newVec;
        }

        Color rendererColor = Renderer.color;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(zero, originalScale, t / duration);
            rendererColor.a = t / duration;
                
            Renderer.color = rendererColor;
            TextMeshPro.alpha = t / duration;
            yield return null;
        }

        transform.localScale = originalScale;
        
        rendererColor.a = 1;
        Renderer.color = rendererColor;

        TextMeshPro.alpha = 1;

        for (int i = 0; i < 20; i++)
        {
            if (HudManager.Instance.Chat.IsOpen && HudManager.Instance.Chat.name == "LoversChatController") break;
            yield return new WaitForSeconds(0.25f);
        }
            
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            rendererColor.a = 1 - t / duration;
            
            Renderer.color = rendererColor;
            TextMeshPro.alpha = 1 - t / duration;
            yield return null;
        }
            
        Destroy(gameObject);
    }
}