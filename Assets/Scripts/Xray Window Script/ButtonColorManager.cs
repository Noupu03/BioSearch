using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BodyButtonManager : MonoBehaviour
{
    [Header("��� ��ư�� ���� �迭�� ��������")]
    public List<Button> bodyButtons = new List<Button>();

    [Header("���� ����")]
    public Color selectedColor = Color.green;

    private Button currentSelected;
    private Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();

    void Start()
    {
        foreach (var btn in bodyButtons)
        {
            // ��ư�� ���� ������ ���� (NormalColor ����)
            originalColors[btn] = btn.colors.normalColor;

            btn.onClick.AddListener(() => OnButtonClicked(btn));
        }
    }

    void OnButtonClicked(Button clickedButton)
    {
        var colors = clickedButton.colors;

       
        // ���� ���� ��ư ���� ������ ����
        if (currentSelected != null)
        {
            RestoreOriginalColor(currentSelected);
        }
        // �������̸� ����
        if (colors.normalColor == Color.red)
            return;

        // ���� ���õ� ��ư ���� ����
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
