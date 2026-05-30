using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BodyButtonManager : MonoBehaviour
{
    [Header("신체 버튼 목록")]
    public List<Button> bodyButtons = new List<Button>();

    [Header("선택 색상")]
    public Color selectedColor = Color.green;

    private Button currentSelected;

    // 클릭 직전 색을 저장 (빨간색이면 빨간색, 흰색이면 흰색을 그대로 기억)
    private readonly Dictionary<Button, Color> restoreColors = new Dictionary<Button, Color>();

    void Start()
    {
        foreach (var btn in bodyButtons)
        {
            Button captured = btn;
            captured.onClick.AddListener(() => OnButtonClicked(captured));
        }
    }

    void OnButtonClicked(Button clickedButton)
    {
        // 같은 버튼 재클릭 → 선택 해제 (원래 색 복원)
        if (currentSelected == clickedButton)
        {
            RestoreColor(currentSelected);
            currentSelected = null;
            return;
        }

        // 이전 선택 버튼을 원래 색으로 복원 (빨간색 → 빨간색, 흰색 → 흰색)
        if (currentSelected != null)
            RestoreColor(currentSelected);

        // 클릭 직전의 현재 색을 복원 색으로 스냅샷
        // (Start 시점이 아닌 클릭 시점에 찍어야 FileWindow 초기화 이후 색이 반영됨)
        restoreColors[clickedButton] = clickedButton.colors.normalColor;

        // 선택 → 초록색
        currentSelected = clickedButton;
        SetButtonColor(currentSelected, selectedColor);
    }

    private void RestoreColor(Button btn)
    {
        if (restoreColors.TryGetValue(btn, out Color orig))
            SetButtonColor(btn, orig);
    }

    private void SetButtonColor(Button btn, Color color)
    {
        ColorBlock cb   = btn.colors;
        cb.normalColor      = color;
        cb.highlightedColor = color;
        cb.pressedColor     = color;
        cb.selectedColor    = color;
        btn.colors = cb;
    }
}
