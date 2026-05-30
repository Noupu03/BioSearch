using UnityEngine;
using TMPro;

/// <summary>
/// ScoreCount 변경 이벤트를 받아 UI를 갱신한다.
/// Update() 폴링 없이 필요할 때만 갱신.
/// </summary>
public class ScoreStageUIManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI successText;
    public TextMeshProUGUI failText;
    public TextMeshProUGUI stageText;

    void OnEnable()
    {
        GameEvents.OnScoreChanged     += UpdateUI;
        GameEvents.OnSceneInitialized += UpdateUI;
    }

    void OnDisable()
    {
        GameEvents.OnScoreChanged     -= UpdateUI;
        GameEvents.OnSceneInitialized -= UpdateUI;
    }

    void Start() => UpdateUI();

    private void UpdateUI()
    {
        if (successText) successText.text = $"성공 : {ScoreCount.successCount}";
        if (failText)    failText.text    = $"실패 : {ScoreCount.failCount}";
        if (stageText)   stageText.text   = $"{ScoreCount.stageCount} 번째";
    }
}
