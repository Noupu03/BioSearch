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
    // 메시지 구조체
    // ────────────────────────────────────────────────
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

        if (notifier != null)
            notifier.DeliverPendingMessages();

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

        if (msg.sender == target || msg.sender == "나")
            RefreshChat();
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
        RefreshChat();

        // 응답 횟수
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
            RefreshChat();
            replyCount[target] = 0;
        }
    }
}

