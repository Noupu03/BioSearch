using System.Collections.Generic;
using UnityEngine;

public class MessengerDataManager : MonoBehaviour
{
    public static MessengerDataManager Instance;

    // 대화 상대 기준으로 메시지 저장
    public Dictionary<string, List<MessengerChatUI.MessageData>> conversations = new Dictionary<string, List<MessengerChatUI.MessageData>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 메시지를 '대화 상대(target)' 기준으로 저장
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
}
