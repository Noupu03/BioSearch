using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 색상을 씬 시작 시 한 번만 적용한다.
/// 색상 변경이 필요하면 Apply()를 직접 호출하거나 OnValidate에서 에디터 미리보기 가능.
/// </summary>
public class GlobalColorManager : MonoBehaviour
{
    public static GlobalColorManager Instance { get; private set; }

    [Header("1. 전체 패널")]
    public Image[] globalPanels;
    public Color   globalColor = Color.black;

    [Header("2. 로그창 패널")]
    public Image[] logPanels;
    public Color   logPanelColor = Color.black;

    [Header("3. 로그창 텍스트")]
    public TMP_Text[] logTexts;
    public TMP_Text[] logPlaceholders;
    public Color      logTextColor = Color.green;

    [Header("4. 파일창 패널")]
    public Image[] filePanels;
    public Color   filePanelColor = Color.black;

    [Header("5. 파일창 텍스트")]
    public TMP_Text[] fileTexts;
    public Color      fileTextColor = Color.cyan;

    [Header("6. 이상 폴더 텍스트 색")]
    public Color abnormalFolderTextColor = Color.red;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Apply();
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    /// <summary>색상을 모든 UI에 적용. 런타임에서 색상을 바꾼 경우 수동 호출.</summary>
    public void Apply()
    {
        foreach (var p in globalPanels)      if (p) p.color = globalColor;
        foreach (var p in logPanels)         if (p) p.color = logPanelColor;
        foreach (var t in logTexts)          if (t) t.color = logTextColor;
        foreach (var t in logPlaceholders)   if (t) t.color = logTextColor;
        foreach (var p in filePanels)        if (p) p.color = filePanelColor;
        foreach (var t in fileTexts)         if (t) t.color = fileTextColor;
    }

#if UNITY_EDITOR
    void OnValidate() => Apply();
#endif
}
