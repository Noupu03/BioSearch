using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectPopupManager : MonoBehaviour
{
    [Header("트리거 버튼")]
    public Button acceptButton;
    public Button releaseButton;

    [Header("팝업 프리팹 / 부모")]
    public GameObject acceptPopupPrefab;
    public GameObject releasePopupPrefab;
    public Transform  popupParent;

    [Header("참조")]
    public SanityManager    sanityManager;
    public LogWindowManager logWindow;
    public FileWindow       fileWindow;

    private SelectPopup currentPopup;

    void Start()
    {
        if (acceptButton)  acceptButton.onClick.AddListener(() => ShowPopup(acceptPopupPrefab, true));
        if (releaseButton) releaseButton.onClick.AddListener(() => ShowPopup(releasePopupPrefab, false));
    }

    private void ShowPopup(GameObject prefab, bool isAccept)
    {
        if (currentPopup != null || prefab == null || popupParent == null) return;

        var go = Instantiate(prefab, popupParent);
        var popup = go.GetComponent<SelectPopup>();
        if (popup == null) return;

        currentPopup = popup;
        popup.OnYesClicked += () => HandleYes(isAccept);
        popup.OnClosed     += () => currentPopup = null;
    }

    private void HandleYes(bool isAccept)
    {
        if (fileWindow == null) return;

        Folder root = fileWindow.GetRootFolder();
        if (root == null) return;

        int  abnormalCount = AbnormalDetector.GetAbnormalCount(root);
        bool success       = (isAccept && abnormalCount == 0) || (!isAccept && abnormalCount > 0);

        if (success)
        {
            logWindow?.Log("성공!");
            ScoreCount.successCount++;
        }
        else
        {
            logWindow?.Log("실패!");
            ScoreCount.failCount++;

            if (sanityManager != null)
            {
                sanityManager.DecreaseSanity(40f);
                if (sanityManager.GetCurrentSanity() <= 0f)
                    return; // SanityManager → GameEvents.OnGameOver → GameOverManager 처리
            }
        }

        ScoreCount.stageCount++;
        GameEvents.RaiseScoreChanged();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
