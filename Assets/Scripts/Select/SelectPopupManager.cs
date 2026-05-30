using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 수락/방류 팝업을 생성하고 판정 결과를 처리한다.
/// FileWindow, LogWindowManager, SanityManager는 Instance로 접근하므로
/// 인스펙터 크로스 참조가 없다.
/// </summary>
public class SelectPopupManager : MonoBehaviour
{
    [Header("트리거 버튼")]
    public Button acceptButton;
    public Button releaseButton;

    [Header("팝업 프리팹 / 부모")]
    public GameObject acceptPopupPrefab;
    public GameObject releasePopupPrefab;
    public Transform  popupParent;

    private SelectPopup currentPopup;

    void Start()
    {
        if (acceptButton)  acceptButton.onClick.AddListener(() => ShowPopup(acceptPopupPrefab, true));
        if (releaseButton) releaseButton.onClick.AddListener(() => ShowPopup(releasePopupPrefab, false));
    }

    private void ShowPopup(GameObject prefab, bool isAccept)
    {
        if (currentPopup != null || prefab == null || popupParent == null) return;

        var go    = Instantiate(prefab, popupParent);
        var popup = go.GetComponent<SelectPopup>();
        if (popup == null) return;

        currentPopup = popup;
        popup.OnYesClicked += () => HandleYes(isAccept);
        popup.OnClosed     += () => currentPopup = null;
    }

    private void HandleYes(bool isAccept)
    {
        var fw = FileWindow.Instance;
        if (fw == null) return;

        Folder root         = fw.GetRootFolder();
        int    abnormal     = AbnormalDetector.GetAbnormalCount(root);
        bool   success      = (isAccept && abnormal == 0) || (!isAccept && abnormal > 0);

        var log = LogWindowManager.Instance;

        if (success)
        {
            log?.Log("성공!");
            ScoreCount.successCount++;
        }
        else
        {
            log?.Log("실패!");
            ScoreCount.failCount++;

            var sanity = SanityManager.Instance;
            if (sanity != null)
            {
                sanity.DecreaseSanity(40f);
                // DecreaseSanity 내부에서 고갈 시 GameEvents.RaiseGameOver 발생
                if (sanity.GetCurrentSanity() <= 0f) return;
            }
        }

        ScoreCount.stageCount++;
        GameEvents.RaiseScoreChanged();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
