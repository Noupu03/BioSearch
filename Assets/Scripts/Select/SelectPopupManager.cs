using Cysharp.Threading.Tasks;
using Haare.Client.UI;
using UnityEngine;
using UnityEngine.UI;

public class SelectPopupManager : MonoBehaviour
{
    [Header("트리거 버튼")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button releaseButton;

    [Header("게임 루프")]
    [SerializeField] private GameLoopManager gameLoopManager;

    private const string AcceptQuestion = "피검사자를 수용하시겠습니까?\n\n이상이 없는 것이 확실합니까?";
    private const string ReleaseQuestion = "피검사자를 방출 하시겠습니까?\n\n이상이 있는 것이 확실합니까?";

    private CoreUIManager uiManager;
    private SelectPopup   currentPopup;

    void Start()
    {
        uiManager = FindObjectOfType<CoreUIManager>();

        if (acceptButton)  acceptButton.onClick.AddListener(() => ShowPopupAsync(true).Forget());
        if (releaseButton) releaseButton.onClick.AddListener(() => ShowPopupAsync(false).Forget());
    }

    private async UniTaskVoid ShowPopupAsync(bool isAccept)
    {
        if (currentPopup != null || uiManager == null) return;

        var question = isAccept ? AcceptQuestion : ReleaseQuestion;
        var panelId  = await uiManager.LoadPanel<SelectPopup, SelectPopupData>(new SelectPopupData { Question = question });
        var popup    = uiManager.RentPanel<SelectPopup>(panelId);
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
