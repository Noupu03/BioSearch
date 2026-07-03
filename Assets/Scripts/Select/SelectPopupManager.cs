using Cysharp.Threading.Tasks;
using Haare.Client.Routine;
using Haare.Client.UI;
using Haare.Util.Logger;
using UnityEngine;
using UnityEngine.UI;

public class SelectPopupManager : MonoRoutine
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
        if (uiManager == null)
            LogHelper.Error(LogHelper.FRAMEWORK, "[SelectPopupManager] CoreUIManager를 찾을 수 없습니다 — GameLifetimeScope.coreUIManagerPrefab이 비어있지 않은지, Tools → 씬 셋업을 실행했는지 확인하세요.");

        if (acceptButton)  acceptButton.onClick.AddListener(() => ShowPopupAsync(true).Forget());
        if (releaseButton) releaseButton.onClick.AddListener(() => ShowPopupAsync(false).Forget());
    }

    private async UniTaskVoid ShowPopupAsync(bool isAccept)
    {
        if (currentPopup != null) return;
        if (uiManager == null)
        {
            LogHelper.Error(LogHelper.FRAMEWORK, "[SelectPopupManager] uiManager가 없어 팝업을 띄울 수 없습니다.");
            return;
        }

        var question = isAccept ? AcceptQuestion : ReleaseQuestion;
        var panelId  = await uiManager.LoadPanel<SelectPopup, SelectPopupData>(new SelectPopupData { Question = question });
        var popup    = uiManager.RentPanel<SelectPopup>(panelId);
        if (popup == null)
        {
            LogHelper.Error(LogHelper.FRAMEWORK, "[SelectPopupManager] SelectPopup 패널 로드에 실패했습니다 — Addressable 등록(Tools → Haare SelectPopup 셋업)을 확인하세요.");
            return;
        }

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
        }

        ScoreCount.NextStage();
        GameEvents.RaiseScoreChanged();
        gameLoopManager?.RequestNextStage();
    }
}
