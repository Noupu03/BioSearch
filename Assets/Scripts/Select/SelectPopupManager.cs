using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectPopupManager : MonoBehaviour
{
    [Header("트리거 버튼")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button releaseButton;

    [Header("팝업 프리팹 / 부모")]
    [SerializeField] private GameObject acceptPopupPrefab;
    [SerializeField] private GameObject releasePopupPrefab;
    [SerializeField] private Transform  popupParent;

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

        int  abnormal = AbnormalDetector.GetAbnormalCount(fw.GetRootFolder());
        bool success  = (isAccept && abnormal == 0) || (!isAccept && abnormal > 0);
        var  log      = LogWindowManager.Instance;

        if (success)
        {
            log?.Log("성공!");
            ScoreCount.AddSuccess();
        }
        else
        {
            log?.Log("실패!");
            ScoreCount.AddFail();

            var sanity = SanityManager.Instance;
            if (sanity != null)
            {
                sanity.DecreaseSanity(40f);
                if (sanity.GetCurrentSanity() <= 0f) return;
            }
        }

        ScoreCount.NextStage();
        GameEvents.RaiseScoreChanged();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
