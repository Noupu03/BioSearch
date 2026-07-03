using Cysharp.Threading.Tasks;
using Haare.Client.Routine;
using Haare.Client.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 질문 텍스트를 데이터로 받는 팝업 뷰. Haare Panel 시스템(CoreUIManager)을 통해
/// 하나의 프리팹으로 수용/방출 두 가지 질문을 모두 표시한다 (SelectPopupData.Question).
/// </summary>
public class SelectPopupData : IPanelData
{
    public string Question;
}

[Panel("SelectPopup")]
public class SelectPopup : MonoRoutine, ICustomPanel
{
    [Header("텍스트")]
    [SerializeField] private TMP_Text questionText;

    [Header("버튼")]
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private Button closeButton;

    public event Action OnYesClicked;
    public event Action OnClosed;

    public SceneUIManager uiManager { get; set; }
    public GameObject panel { get; set; }

    protected override void Constructor()
    {
        base.Constructor();
        panel = gameObject;
    }

    void Start()
    {
        if (yesButton)   yesButton.onClick.AddListener(HandleYes);
        if (noButton)    noButton.onClick.AddListener(HandleClose);
        if (closeButton) closeButton.onClick.AddListener(HandleClose);
    }

    public async UniTask SetData(IPanelData data)
    {
        if (data is SelectPopupData popupData && questionText != null)
            questionText.text = popupData.Question;

        await UniTask.CompletedTask;
    }

    // SceneUIManager.Register()가 프리팹을 비활성 상태로 인스턴스화하므로,
    // LoadPanel 완료 시 매니저가 호출하는 이 지점에서 직접 활성화해야 한다.
    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }

    private void HandleYes()
    {
        OnYesClicked?.Invoke();
        HandleClose();
    }

    private void HandleClose()
    {
        OnClosed?.Invoke();
        uiManager?.ClosePanel<SelectPopup>();
    }
}
