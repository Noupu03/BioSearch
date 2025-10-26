using UnityEngine;
using TMPro;

public class ScoreStageUIManager : MonoBehaviour
{
    [Header("������ TMP Text")]
    public TextMeshProUGUI successText;
    public TextMeshProUGUI failText;
    public TextMeshProUGUI stageText;
    

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        //SelectPopupManager spm = FindObjectOfType<SelectPopupManager>();
        //if (scoreCount == null) return;

        if (successText != null)
            successText.text = $"���� : {ScoreCount.successCount}";

        if (failText != null)
            failText.text = $"���� : {ScoreCount.failCount}";

        if (stageText != null)
            stageText.text = $"{ScoreCount.stageCount} ��°";
    }
}
