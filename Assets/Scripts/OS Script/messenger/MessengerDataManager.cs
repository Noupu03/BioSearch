using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class MessengerDataManager : MonoBehaviour
{
    public static MessengerDataManager Instance;

    // 대화 상대 기준으로 메시지 저장
    // ★ List<object> → List<MessengerChatUI.MessageData> 로 변경
    public Dictionary<string, List<MessengerChatUI.MessageData>> conversations
        = new Dictionary<string, List<MessengerChatUI.MessageData>>();

    // 메시지 키 → 매핑 정보
    private Dictionary<string, MessageMapping> mappings = new Dictionary<string, MessageMapping>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ────────────────────────────────────────────────
    // 메시지를 대화 상대(target) 기준으로 저장
    // object → MessageData 로 타입 고정
    // ────────────────────────────────────────────────
    public void AddMessage(MessengerChatUI.MessageData msg, string target)
    {
        if (!conversations.ContainsKey(target))
            conversations[target] = new List<MessengerChatUI.MessageData>();

        conversations[target].Add(msg);
    }

    // ────────────────────────────────────────────────
    // 대화 리스트 반환
    // List<object> → List<MessageData>
    // ────────────────────────────────────────────────
    public List<MessengerChatUI.MessageData> GetConversation(string target)
    {
        if (conversations.ContainsKey(target))
            return conversations[target];

        return new List<MessengerChatUI.MessageData>();
    }

    // ────────────────────────────────────────────────
    // 매핑 등록
    // ────────────────────────────────────────────────
    public void RegisterMapping(string timeStr, string sender, string text, string checkListText, string linkedFileName)
    {
        string key = GenerateMessageKey(timeStr, sender, text);

        if (!mappings.ContainsKey(key))
        {
            mappings[key] = new MessageMapping
            {
                key = key,
                timeString = timeStr,
                sender = sender,
                text = text,
                checkListText = checkListText,
                linkedFileName = linkedFileName,
                addedToUI = false,
                completed = false
            };
        }
        else
        {
            mappings[key].checkListText = checkListText;
            mappings[key].linkedFileName = linkedFileName;
        }

#if UNITY_EDITOR
        Debug.Log($"[MessengerDataManager] Registered mapping: {key} -> checklist:'{checkListText}', file:'{linkedFileName}'");
#endif
    }

    // ────────────────────────────────────────────────
    // Open Chat시 해당 대상의 메시지 스캔 → 매핑된 체크리스트 생성
    // ────────────────────────────────────────────────
    public void CreateChecklistsForTarget(string target)
    {
        var conv = GetConversation(target);

        foreach (var msg in conv)
        {
            string timeStr = msg.dateTime.ToString();
            string sender = msg.sender;
            string text = msg.content;

            string key = GenerateMessageKey(timeStr, sender, text);

            if (mappings.ContainsKey(key))
            {
                var map = mappings[key];

                if (!map.addedToUI && !string.IsNullOrEmpty(map.checkListText))
                {
                    if (CheckList.Instance != null)
                    {
                        CheckList.Instance.AddCheckFromMessage(map.key, map.checkListText, map.linkedFileName);
                        map.addedToUI = true;

#if UNITY_EDITOR
                        Debug.Log($"[MessengerDataManager] Added checklist for message {map.key}");
#endif
                    }
                }
            }
        }
    }

    // ────────────────────────────────────────────────
    // 파일 열었을 때 해당 체크리스트 완료 처리
    // ────────────────────────────────────────────────
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

    class MessageMapping
    {
        public string key;
        public string timeString;
        public string sender;
        public string text;
        public string checkListText;
        public string linkedFileName;

        public bool addedToUI;
        public bool completed;
    }
}
