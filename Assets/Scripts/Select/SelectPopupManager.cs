using UnityEngine;
using UnityEngine.UI;

public class SelectPopupManager : MonoBehaviour
{
    [Header("트리거 버튼")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button releaseButton;

    [Header("팝업 프리팹 / 부모")]
    [SerializeField] private GameObject acceptPopupPrefab;
    [SerializeField] private GameObject releasePopupPrefab;
    [SerializeField] private Transform  popupParent;

    [Header("게임 루프")]
    [SerializeField] private GameLoopManager gameLoopManager;

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
            SanityManager.Instance?.DecreaseSanity(40f);
            // 게임오버 여부는 SanityManager → GameEvents.OnGameOver → GameLoopManager가 처리
            // _isTransitioning 플래그로 RequestNextStage 중복 실행이 차단됨
        }

        ScoreCount.NextStage();
        GameEvents.RaiseScoreChanged();
        gameLoopManager?.RequestNextStage();
    }
}
