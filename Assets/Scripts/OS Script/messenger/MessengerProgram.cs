using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessengerProgram : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_Text chatContent;
    public TMP_Text currentTargetText;
    public ScrollRect scrollRect;

    [Header("대화 전환 버튼")]
    public Button bossButton;
    public Button predecessorButton;

    [Header("노티파이어 연결")]
    public MessengerNotifier notifier;

    [Header("설정")]
    public string currentTarget = "상사";

    private void Start()
    {
        if (bossButton != null) bossButton.onClick.AddListener(() => ShowConversation("상사"));
        if (predecessorButton != null) predecessorButton.onClick.AddListener(() => ShowConversation("전임자"));
    }

    private void OnEnable()
    {
        // 비활성화 후 활성화될 때 누적 메시지 전달
        if (notifier != null)
            notifier.DeliverPendingMessages();
    }

    // 외부에서 메시지 수신
    public void ReceiveExternalMessage(MessageData msg)
    {
        // 데이터 저장
        MessengerDataManager.Instance.AddMessage(msg);

        // 팝업 처리
        if (notifier != null)
            notifier.HandleMessageArrival(msg);

        // UI에 표시
        if (gameObject.activeInHierarchy)
        {
            DisplayMessage(msg);
        }
    }

    private void DisplayMessage(MessageData msg)
    {
        if (msg.sender == currentTarget)
            AppendMessageToUI(msg);

        ScrollToBottom();
    }

    public void ShowConversation(string target)
    {
        currentTarget = target;
        currentTargetText.text = target;
        chatContent.text = "";

        foreach (var msg in MessengerDataManager.Instance.GetConversation(target))
        {
            AppendMessageToUI(msg);
        }

        ScrollToBottom();
    }

    private void AppendMessageToUI(MessageData msg)
    {
        if (msg.sender == "나")
            chatContent.text += $"[{msg.sender}] {msg.content}\n";
        else
            chatContent.text += $"[{msg.sender}] {msg.content} ({msg.dateTime})\n";
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    [System.Serializable]
    public struct MessageData
    {
        public GameDateTime dateTime;
        public string sender;
        public string content;

        public MessageData(GameDateTime dt, string sender, string content)
        {
            dateTime = dt;
            this.sender = sender;
            this.content = content;
        }
    }
}
