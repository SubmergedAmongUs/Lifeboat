using System;
using System.Linq;
using System.Text;
using Framework.Attributes;
using Framework.Extensions;
using Framework.Localization;
using TMPro;
using UnityEngine;

namespace Lifeboat.WinScreen.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class SummaryText : MonoBehaviour
{
    public SummaryText(IntPtr ptr) : base(ptr)
    {
    }

    private float Timer = 0f;
    private float MaxTime = 3f;
    private TextMeshPro Text;
    public void Awake()
    {
        Text = GetComponent<TextMeshPro>();
        Text.alignment = TextAlignmentOptions.TopLeft;
        Text.color = Color.white;
        Text.fontSizeMin = 1f;
        Text.fontSizeMax = 1f;
        Text.fontSize = 1f;
        Text.autoSizeTextContainer = true;
            
        StringBuilder builder = new();
        builder.AppendLine(LanguageProvider.Current.Lifeboat_UI_EndScreen_GameSummary);
        builder.AppendLine();
        bool ウイナーがいますか = false;
        foreach ((_, string s) in TempWinData.Current.RoleData.Where(o => o.name.StartsWith("<color=green>")).OrderBy(o => o.name))
        {
            builder.AppendLine(s);
            ウイナーがいますか = true;
        }
        if (ウイナーがいますか) builder.AppendLine();
        foreach ((_, string s) in TempWinData.Current.RoleData.Where(o => !o.name.StartsWith("<color=green>")).OrderBy(o => o.name))
        {
            builder.AppendLine(s);
        }

        Text.text = builder.ToString();

        RectTransform rect = Text.rectTransform;
        rect.anchoredPosition = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 1);
            
        AspectPosition aspect = gameObject.AddComponent<AspectPosition>();
        aspect.Alignment = AspectPosition.EdgeAlignments.LeftTop;
        aspect.DistanceFromEdge = new Vector3(0.1f, 0.1f, 0);
        aspect.AdjustPosition();

        transform.SetZPos(-100);
    }

    private void Update()
    {
        Timer += Time.deltaTime;
        Text.alpha = Mathf.Clamp01(Timer / MaxTime);
    }
}