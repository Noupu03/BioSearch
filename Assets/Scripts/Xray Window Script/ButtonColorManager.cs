using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BodyButtonManager : MonoBehaviour
{
    [Header("모든 버튼을 여기 배열에 넣으세요")]
    public List<Button> bodyButtons = new List<Button>();

    [Header("선택 색상")]
    public Color selectedColor = Color.green;

    private Button currentSelected;
    private Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();

    void Start()
    {
        foreach (var btn in bodyButtons)
        {
            // 버튼의 현재 색상을 저장 (NormalColor 기준)
            originalColors[btn] = btn.colors.normalColor;

            btn.onClick.AddListener(() => OnButtonClicked(btn));
        }
    }

    void OnButtonClicked(Button clickedButton)
    {
        var colors = clickedButton.colors;

       
        // 이전 선택 버튼 원래 색으로 복원
        if (currentSelected != null)
        {
            RestoreOriginalColor(currentSelected);
        }
        // 빨간색이면 무시
        if (colors.normalColor == Color.red)
            return;

        // 새로 선택된 버튼 색상 변경
        currentSelected = clickedButton;
        SetButtonColor(currentSelected, selectedColor);
    }

    void SetButtonColor(Button btn, Color color)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor = color;
        cb.highlightedColor = color;
        cb.pressedColor = color;
        cb.selectedColor = color;
        btn.colors = cb;
    }

    void RestoreOriginalColor(Button btn)
    {
        if (!originalColors.ContainsKey(btn)) return;

        Color orig = originalColors[btn];
        SetButtonColor(btn, orig);
    }
}
