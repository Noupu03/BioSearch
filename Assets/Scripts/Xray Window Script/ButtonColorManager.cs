using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonColorManager : MonoBehaviour
{
    [Header("모든 버튼을 여기 배열에 넣으세요")]
    public List<Button> bodyButtons = new List<Button>();

    [Header("선택 색상")]
    public Color selectedColor = Color.green;
    [Header("에러 색상")]
    public Color errorColor = Color.red;

    private Button currentSelected;
    private Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();

    void Start()
    {
        foreach (var btn in bodyButtons)
        {
            if (btn == null) continue;

            // 버튼의 현재 색상을 저장 (NormalColor 기준)
            originalColors[btn] = btn.colors.normalColor;

            btn.onClick.AddListener(() => OnButtonClicked(btn));
        }
    }

    void OnButtonClicked(Button clickedButton)
    {
        if (clickedButton == null) return;

        // 빨간색(고장 버튼)이면 선택 불가
        if (clickedButton.colors.normalColor == errorColor)
            return;

        // 이전 선택 버튼 원래 색으로 복원
        if (currentSelected != null)
            RestoreOriginalColor(currentSelected);

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
        if (btn == null || !originalColors.ContainsKey(btn)) return;

        Color orig = originalColors[btn];
        SetButtonColor(btn, orig);
    }

    /// <summary>
    /// 외부에서 고장 버튼 색상을 갱신하도록 호출
    /// </summary>
    public void UpdateErrorButtonColors(Dictionary<string, bool> errorStates)
    {
        foreach (var btn in bodyButtons)
        {
            if (btn == null) continue;
            string btnName = btn.name;

            if (errorStates.TryGetValue(btnName, out bool isError) && isError)
            {
                // 에러 상태: 빨간색 표시
                SetButtonColor(btn, errorColor);
            }
            else
            {
                // 정상 상태: 원래 색으로 복원 (선택된 버튼이 아니면)
                if (btn != currentSelected)
                    RestoreOriginalColor(btn);
            }
        }
    }
}
