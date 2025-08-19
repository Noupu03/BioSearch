using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GlobalColorManager : MonoBehaviour
{
    [Header("Panels (��� ��)")]
    public Image[] panels;

    [Header("Texts (�α�, �Է� �ؽ�Ʈ)")]
    public TMP_Text[] texts;

    [Header("Placeholders (�Է� �ʵ� ��Ʈ �ؽ�Ʈ)")]
    public TMP_Text[] placeholders;

    [Header("Colors")]
    public Color panelAndPlaceholderColor = Color.black; // Panel + Placeholder ����
    public Color textColor = Color.green;                // Text�� ����

    private void Update()
    {
        ApplyColors();
    }

    private void ApplyColors()
    {
        // Panels + Placeholder ���� ���� ����
        foreach (var panel in panels)
        {
            if (panel != null)
                panel.color = panelAndPlaceholderColor;
        }

        foreach (var placeholder in placeholders)
        {
            if (placeholder != null)
                placeholder.color = panelAndPlaceholderColor;
        }

        // Texts ����
        foreach (var text in texts)
        {
            if (text != null)
                text.color = textColor;
        }
    }

    // ��Ÿ�ӿ��� ���� ����
    public void SetPanelAndPlaceholderColor(Color color)
    {
        panelAndPlaceholderColor = color;
    }

    public void SetTextColor(Color color)
    {
        textColor = color;
    }
}
