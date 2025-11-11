using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;

public class MessengerProgram : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_Text chatContent;
    public TMP_Text currentTargetText;
    public ScrollRect scrollRect;
    public Button replyButton; // ▼ 추가됨: 대답하기 버튼

    [Header("시간 매니저 연결")]
    public OSTimeManager timeManager;

    [Header("노티파이어 연결")]
    public MessengerNotifier notifier;

    [Header("설정")]
    public string currentTarget = "상사";

    [Header("상단 전환 버튼")]
    public Button bossButton;
    public Button predecessorButton;

    [Header("채팅창 프리팹")]
    public GameObject chatWindowPrefab;

    // 대화 저장용
    private Dictionary<string, List<MessageData>> conversations = new Dictionary<string, List<MessageData>>();

    // 외부 예약 메시지 접근용
    public List<MessageData> scheduledMessages = new List<MessageData>();

    // 응답 횟수 추적
    private Dictionary<string, int> replyCount = new Dictionary<string, int>();

    ProgramOpen programopen;

    private IEnumerator Start()
    {
        yield return null; 

        if (programopen == null)
        {
            programopen = FindObjectOfType<ProgramOpen>(); 
            if (programopen == null)
            {
                Debug.LogError("ProgramOpen 인스턴스를 찾을 수 없습니다!");
                yield break;
            }
        }

        // 버튼 연결
        conversations["상사"] = new List<MessageData>();
        conversations["전임자"] = new List<MessageData>();
        conversations[""] = new List<MessageData>();

        if (replyButton != null)
            replyButton.onClick.AddListener(OnReplyButtonClicked);

        if (bossButton != null)
        {
            bossButton.onClick.AddListener(() =>
            {
                Debug.Log("[버튼] 상사 대화 열기 요청됨");
                programopen.MessangerWindowOpen();
                ShowConversation("상사");
            });
        }

        if (predecessorButton != null)
        {
            predecessorButton.onClick.AddListener(() =>
            {
                Debug.Log("[버튼] 전임자 대화 열기 요청됨");
                programopen.MessangerWindowOpen();
                ShowConversation("전임자");
            });
        }
    }


    void Update()
    {
        if (timeManager == null) return;
        GameDateTime now = timeManager.GetCurrentGameTime();

        for (int i = scheduledMessages.Count - 1; i >= 0; i--)
        {
            var msg = scheduledMessages[i];
            if (msg.dateTime <= now)
            {
                ReceiveExternalMessage(msg);
                scheduledMessages.RemoveAt(i);
            }
        }
    }

    void OnEnable()
    {
        if (notifier != null)
        {
            notifier.DeliverPendingMessages();
        }
    }

    public void ShowConversation(string target)
    {
        if (!conversations.ContainsKey(target)) return;
        currentTarget = target;
        currentTargetText.text = target;

        chatContent.text = "";
        foreach (var msg in conversations[target])
            AppendMessageToUI(msg);

        ScrollToBottom();
    }

    // 메시지 수신 (상사나 전임자)
    public void ReceiveExternalMessage(MessageData msg)
    {
        if (!conversations.ContainsKey(msg.sender))
            conversations[msg.sender] = new List<MessageData>();
        conversations[msg.sender].Add(msg);

        // 새 메시지 도착 → 응답 횟수 초기화
        replyCount[msg.sender] = 0;

        if (notifier != null)
            notifier.HandleMessageArrival(msg);

        if (gameObject.activeInHierarchy)
        {
            DisplayMessage(msg);
        }
    }

    // 실제 UI 표시
    void DisplayMessage(MessageData msg)
    {
        if (msg.sender == currentTarget)
            AppendMessageToUI(msg);

        ScrollToBottom();
    }

    // 대답하기 버튼 눌렀을 때
    void OnReplyButtonClicked()
    {
        if (string.IsNullOrEmpty(currentTarget)) return;

        // 내 메시지 추가
        MessageData myReply = new MessageData(timeManager.GetCurrentGameTime(), "나", "네");
        conversations[currentTarget].Add(myReply);

        // 대화창에 표시 (날짜 없음)
        AppendMessageToUI(myReply);
        ScrollToBottom();

        // 응답 횟수 증가
        if (!replyCount.ContainsKey(currentTarget))
            replyCount[currentTarget] = 0;

        replyCount[currentTarget]++;

        // 2회 이상 대답하면 상대의 반응 생성
        if (replyCount[currentTarget] >= 2)
        {
            GameDateTime now = timeManager.GetCurrentGameTime();
            string sender = currentTarget;
            MessageData warnMsg = new MessageData(now, sender, "한번만 대답해도 된다.");

            conversations[sender].Add(warnMsg);
            if (sender == currentTarget)
                AppendMessageToUI(warnMsg);

            ScrollToBottom();

            // 2회 경고 후 카운트 초기화
            replyCount[currentTarget] = 0;
        }
    }

    public void AppendMessageToUI(MessageData msg)
    {
        if (msg.sender == "나")
        {
            chatContent.text += $"[{msg.sender}] : {msg.content}\n";
        }
        else
        {
            chatContent.text += $"[{msg.sender}] : {msg.content} ({msg.dateTime})\n";
        }
    }

    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public struct MessageData
    {
        public GameDateTime dateTime;
        public string sender;
        public string content;

        public MessageData(GameDateTime dt, string sender, string content)
        {
            this.dateTime = dt;
            this.sender = sender;
            this.content = content;
        }
    }
}
