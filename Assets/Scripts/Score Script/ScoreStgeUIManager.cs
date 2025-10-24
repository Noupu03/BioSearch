using UnityEngine;
using TMPro;

public class ScoreStageUIManager : MonoBehaviour
{
    [Header("연동할 TMP Text")]
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
            successText.text = $"성공 : {SelectPopupManager.successCount}";

        if (failText != null)
            failText.text = $"실패 : {SelectPopupManager.failCount}";

        if (stageText != null)
            stageText.text = $"{SelectPopupManager.stageCount} 명째";
    }
}
