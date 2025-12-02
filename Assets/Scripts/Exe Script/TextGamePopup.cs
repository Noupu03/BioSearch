using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TextGamePopup : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text gameText;
    public Button closeButton;
    public RectTransform topBar; // Inspector에서 드래그용 탑바 연결

    private SnakeGame snakeGame;
    private bool isRunningGame = false;

    // 드래그용
    private bool isDragging = false;
    private Vector2 offset;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // 탑바가 있다면 드래그 이벤트 연결
        if (topBar != null)
        {
            // EventTrigger를 동적으로 추가
            EventTrigger trigger = topBar.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = topBar.gameObject.AddComponent<EventTrigger>();

            // BeginDrag
            EventTrigger.Entry beginDrag = new EventTrigger.Entry();
            beginDrag.eventID = EventTriggerType.BeginDrag;
            beginDrag.callback.AddListener((data) => { OnBeginDrag((PointerEventData)data); });
            trigger.triggers.Add(beginDrag);

            // Drag
            EventTrigger.Entry drag = new EventTrigger.Entry();
            drag.eventID = EventTriggerType.Drag;
            drag.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
            trigger.triggers.Add(drag);
        }

        // 닫기 버튼
        if (closeButton != null)
            closeButton.onClick.AddListener(() => Destroy(gameObject));
    }

    public void Open(string title, string content)
    {
        gameText.text = "Loading...";

        if (content == "snake")
        {
            StartSnakeGame();
        }
    }

    private void StartSnakeGame()
    {
        isRunningGame = true;
        snakeGame = new SnakeGame(gameText);
        snakeGame.Init();
    }

    private void Update()
    {
        if (isRunningGame && snakeGame != null)
            snakeGame.Update();
    }

    // ---------------------
    // 드래그 처리 함수
    // ---------------------
    private void OnBeginDrag(PointerEventData data)
    {
        // 마우스와 팝업 위치 차이 저장
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            data.position,
            data.pressEventCamera,
            out Vector2 localPointerPosition
        );

        offset = rectTransform.anchoredPosition - localPointerPosition;
    }

    private void OnDrag(PointerEventData data)
    {
        if (rectTransform == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            data.position,
            data.pressEventCamera,
            out Vector2 localPointerPosition
        );

        rectTransform.anchoredPosition = localPointerPosition + offset;
    }
}
