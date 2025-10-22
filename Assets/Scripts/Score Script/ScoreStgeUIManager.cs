using UnityEngine;
using TMPro;

public class ScoreStageUIManager : MonoBehaviour
{
    [Header("¿¬µ¿ÇÒ TMP Text")]
    public TextMeshProUGUI successText;
    public TextMeshProUGUI failText;
    public TextMeshProUGUI stageText;

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        SelectPopupManager spm = FindObjectOfType<SelectPopupManager>();
        if (spm == null) return;

        if (successText != null)
            successText.text = $"Success: {SelectPopupManager.successCount}";

        if (failText != null)
            failText.text = $"Fail: {SelectPopupManager.failCount}";

        if (stageText != null)
            stageText.text = $"Stage: {SelectPopupManager.stageCount}";
    }
}
