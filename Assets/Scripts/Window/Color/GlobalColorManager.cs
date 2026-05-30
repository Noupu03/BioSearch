using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GlobalColorManager : MonoBehaviour
{
    [Header("1. Scene 전체")]
    public Image[] globalPanels;
    public Color globalColor = Color.black;

    [Header("2. 로그창 Panels")]
    public Image[] logPanels;
    public Color logPanelColor = Color.black;

    [Header("3. 로그창 Texts (로그 + 입력필드 텍스트)")]
    public TMP_Text[] logTexts;
    public TMP_Text[] logPlaceholders;
    public Color logTextColor = Color.green;

    [Header("4. 파일창 Panels")]
    public Image[] filePanels;
    public Color filePanelColor = Color.black;

    [Header("5. 파일창 Texts")]
    public TMP_Text[] fileTexts;
    public Color fileTextColor = Color.cyan;

    [Header("6. 이상 폴더 Texts")]
    public Color abnormalFolderTextColor = Color.red;

    private void Update()
    {
        ApplyColors();
    }

    private void ApplyColors()
    {
        // 1. Scene 전체
        foreach (var panel in globalPanels) if (panel != null) panel.color = globalColor;

        // 2. 로그창 Panels
        foreach (var panel in logPanels) if (panel != null) panel.color = logPanelColor;

        // 3. 로그창 Texts
        foreach (var text in logTexts) if (text != null) text.color = logTextColor;
        foreach (var placeholder in logPlaceholders) if (placeholder != null) placeholder.color = logTextColor;

        // 4. 파일창 Panels
        foreach (var panel in filePanels) if (panel != null) panel.color = filePanelColor;

        // 5. 파일창 Texts (정상 폴더)
        foreach (var text in fileTexts) if (text != null) text.color = fileTextColor;

        // 6. 이상 폴더 텍스트는 FileIcon이 직접 적용
    }

    // 런타임에서 색상 변경
    public void SetAbnormalFolderTextColor(Color color) => abnormalFolderTextColor = color;
}
