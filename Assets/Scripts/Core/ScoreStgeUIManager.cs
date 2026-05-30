using UnityEngine;
using TMPro;

public class ScoreStageUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI successText;
    [SerializeField] private TextMeshProUGUI failText;
    [SerializeField] private TextMeshProUGUI stageText;

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
        if (successText) successText.text = $"성공 : {ScoreCount.SuccessCount}";
        if (failText)    failText.text    = $"실패 : {ScoreCount.FailCount}";
        if (stageText)   stageText.text   = $"{ScoreCount.StageCount} 번째";
    }
}
