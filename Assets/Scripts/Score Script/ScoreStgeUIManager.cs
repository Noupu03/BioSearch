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
        //SelectPopupManager spm = FindObjectOfType<SelectPopupManager>();
        //if (scoreCount == null) return;

        if (successText != null)
            successText.text = $"성공 : {ScoreCount.successCount}";

        if (failText != null)
            failText.text = $"실패 : {ScoreCount.failCount}";

        if (stageText != null)
            stageText.text = $"{ScoreCount.stageCount} 명째";
    }
}
