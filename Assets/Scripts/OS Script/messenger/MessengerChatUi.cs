using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MessengerChatUI : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_Text currentTargetText;
    public TMP_Text chatContent;
    public Button replyButton;

    [Header("시간 매니저 연결 (자동 연결됨)")]
    public OSTimeManager timeManager;

    [Header("알림 매니저 연결 (자동 연결됨)")]
    public MessengerNotifier notifier;

    [HideInInspector]
    public string target;

    private Dictionary<string, int> replyCount = new Dictionary<string, int>();

    // ────────────────────────────────────────────────
    // 메시지 구조체 (체크리스트 + 파일 매핑 추가됨)
    // ────────────────────────────────────────────────
    public struct MessageData
    {
        public GameDateTime dateTime;
        public string sender;
        public string content;

        // 새로 추가된 필드들
        public string checkListText;     // 체크리스트에 표시할 텍스트
        public string linkedFileName;    // 해당 파일 열면 체크 완료
        public bool checklistAdded;      // UI에 체크리스트 반영되었는지 여부

        public MessageData(
            GameDateTime dt,
            string sender,
            string content,
            string checkListText = "",
            string linkedFileName = ""
        )
        {
            dateTime = dt;
            this.sender = sender;
            this.content = content;

            this.checkListText = checkListText;
            this.linkedFileName = linkedFileName;
            this.checklistAdded = false;
        }
    }

    private void Start()
    {
        if (replyButton != null)
            replyButton.onClick.AddListener(OnReplyButtonClicked);

        if (timeManager == null)
            timeManager = FindObjectOfType<OSTimeManager>();

        if (notifier == null)
            notifier = FindObjectOfType<MessengerNotifier>();
    }

    public void SetTarget(string newTarget)
    {
        target = newTarget;

        if (currentTargetText != null)
            currentTargetText.text = target;

        // 메신저 알림 처리
        if (notifier != null)
            notifier.DeliverPendingMessages();

        // 체크리스트 생성 (openChat 시점에 생성됨)
        MessengerDataManager.Instance.CreateChecklistsForTarget(target);

        RefreshChat();
    }

    public void RefreshChat()
    {
        if (string.IsNullOrEmpty(target)) return;

        if (currentTargetText != null)
            currentTargetText.text = target;

        chatContent.text = "";

        List<MessageData> allMessages = MessengerDataManager.Instance.GetConversation(target);

        foreach (var msg in allMessages)
        {
            if (msg.sender == "나")
                chatContent.text += $"[나] : {msg.content}\n";
            else
                chatContent.text += $"[{msg.sender}] : {msg.content} ({msg.dateTime})\n";
        }
    }

    public void OnNewMessageReceived(MessageData msg)
    {
        string conversationKey = (msg.sender == "나") ? target : msg.sender;
        MessengerDataManager.Instance.AddMessage(msg, conversationKey);

        // 받은 메시지가 현재 보고있는 대상이면 즉시 갱신
        if (msg.sender == target || msg.sender == "나")
        {
            // 메시지 도착 → 체크리스트 매핑 메시지인지 확인
            MessengerDataManager.Instance.CreateChecklistsForTarget(target);
            RefreshChat();
        }
    }

    private void OnReplyButtonClicked()
    {
        if (string.IsNullOrEmpty(target))
        {
            Debug.LogWarning("MessengerChatUI: 대상(target)이 설정되지 않았습니다.");
            return;
        }

        if (timeManager == null)
            timeManager = FindObjectOfType<OSTimeManager>();

        if (timeManager == null)
        {
            Debug.LogWarning("MessengerChatUI: OSTimeManager를 찾을 수 없어 대답 불가");
            return;
        }

        // 내 메시지
        MessageData myReply = new MessageData(
            timeManager.GetCurrentGameTime(),
            "나",
            "네"
        );

        MessengerDataManager.Instance.AddMessage(myReply, target);
        MessengerDataManager.Instance.CreateChecklistsForTarget(target);
        RefreshChat();

        // 응답 횟수 관리
        if (!replyCount.ContainsKey(target))
            replyCount[target] = 0;

        replyCount[target]++;

        // 자동 응답
        if (replyCount[target] >= 2)
        {
            GameDateTime now = timeManager.GetCurrentGameTime();
            MessageData warnMsg = new MessageData(
                now,
                target,
                "한번만 대답해도 된다."
            );

            MessengerDataManager.Instance.AddMessage(warnMsg, target);
            MessengerDataManager.Instance.CreateChecklistsForTarget(target);
            RefreshChat();
            replyCount[target] = 0;
        }
    }
}
