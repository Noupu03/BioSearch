using System;
using System.Collections.Generic;
using UnityEngine;

public class MessengerDataManager : MonoBehaviour
{
    public static MessengerDataManager Instance;

    // 대화 상대 기준 메시지 저장
    public Dictionary<string, List<MessengerChatUI.MessageData>> conversations
        = new Dictionary<string, List<MessengerChatUI.MessageData>>();

    // 메시지 키 → 매핑 정보
    private Dictionary<string, MessageMapping> mappings = new Dictionary<string, MessageMapping>();
    public OSTimeManager timeManager; // 인스펙터에서 할당
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ────────────────────────────────────────────────
    // 메시지를 대화 상대 기준으로 저장
    // ────────────────────────────────────────────────
    public void AddMessage(MessengerChatUI.MessageData msg, string target)
    {
        if (!conversations.ContainsKey(target))
            conversations[target] = new List<MessengerChatUI.MessageData>();

        conversations[target].Add(msg);
    }

    public List<MessengerChatUI.MessageData> GetConversation(string target)
    {
        if (conversations.ContainsKey(target))
            return conversations[target];
        return new List<MessengerChatUI.MessageData>();
    }

    // ────────────────────────────────────────────────
    // 매핑 등록
    // ────────────────────────────────────────────────
    public void RegisterMapping(GameDateTime dateTime, string sender, string text, string checkListText, string linkedFileName)
    {
        string key = GenerateMessageKey(dateTime.ToString(), sender, text);

        if (!mappings.ContainsKey(key))
        {
            mappings[key] = new MessageMapping
            {
                key = key,
                sender = sender,
                text = text,
                checkListText = checkListText,
                linkedFileName = linkedFileName,
                addedToUI = false,
                completed = false,
                dateTime = dateTime
            };
        }
        else
        {
            mappings[key].checkListText = checkListText;
            mappings[key].linkedFileName = linkedFileName;
            mappings[key].dateTime = dateTime;
        }

#if UNITY_EDITOR
        Debug.Log($"[MessengerDataManager] Registered mapping: {key} -> checklist:'{checkListText}', file:'{linkedFileName}'");
#endif
    }

    // ────────────────────────────────────────────────
    // Open Chat시 체크리스트 생성
    // ────────────────────────────────────────────────
    public void CreateChecklistsForTarget(string target)
    {
        if (!conversations.ContainsKey(target)) return;

        GameDateTime now = timeManager.GetCurrentGameTime(); // 현재 시간 가져오기
        var conv = conversations[target];

        foreach (var msg in conv)
        {
            string key = GenerateMessageKey(msg.dateTime.ToString(), msg.sender, msg.content);

            if (mappings.ContainsKey(key))
            {
                var map = mappings[key];

                // 하루 지난 메시지는 체크리스트에 추가하지 않음
                bool isSameDay = map.dateTime.year == now.year &&
                                 map.dateTime.month == now.month &&
                                 map.dateTime.day == now.day;

                if (!map.addedToUI && !string.IsNullOrEmpty(map.checkListText) && isSameDay)
                {
                    if (CheckList.Instance != null)
                    {
                        CheckList.Instance.AddCheckFromMessage(
                            map.key,
                            map.checkListText,
                            map.linkedFileName,
                            map.dateTime
                        );
                        map.addedToUI = true;

#if UNITY_EDITOR
                        Debug.Log($"[MessengerDataManager] Added checklist for message {map.key}");
#endif
                    }
                }
            }
        }
    }


    public void MarkMappingCompletedByFile(string fileName)
    {
        foreach (var kv in mappings)
        {
            var m = kv.Value;
            if (!m.completed && !string.IsNullOrEmpty(m.linkedFileName))
            {
                if (string.Equals(m.linkedFileName, fileName, StringComparison.OrdinalIgnoreCase))
                {
                    m.completed = true;
                }
            }
        }
    }

    // -------------- Helper Methods ------------------
    private string GenerateMessageKey(string timeStr, string sender, string text)
    {
        timeStr = timeStr ?? "";
        sender = sender ?? "";
        text = text ?? "";
        return $"{timeStr}|{sender}|{text}";
    }

    // ────────────────────────────────────────────────
    // 메시지 매핑 구조체
    // ────────────────────────────────────────────────
    class MessageMapping
    {
        public string key;
        public string sender;
        public string text;
        public string checkListText;
        public string linkedFileName;
        public bool addedToUI;
        public bool completed;
        public GameDateTime dateTime; // 메시지 시간
    }
}
