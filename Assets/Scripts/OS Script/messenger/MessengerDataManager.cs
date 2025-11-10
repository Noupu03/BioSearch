using System.Collections.Generic;
using UnityEngine;

public class MessengerDataManager : MonoBehaviour
{
    public static MessengerDataManager Instance;

    // 전체 대화 목록
    public Dictionary<string, List<MessengerProgram.MessageData>> conversations
        = new Dictionary<string, List<MessengerProgram.MessageData>>();

    // 예약 메시지
    public List<MessengerProgram.MessageData> scheduledMessages
        = new List<MessengerProgram.MessageData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddMessage(MessengerProgram.MessageData msg)
    {
        if (!conversations.ContainsKey(msg.sender))
            conversations[msg.sender] = new List<MessengerProgram.MessageData>();
        conversations[msg.sender].Add(msg);
    }

    public List<MessengerProgram.MessageData> GetConversation(string sender)
    {
        if (conversations.ContainsKey(sender))
            return conversations[sender];
        return new List<MessengerProgram.MessageData>();
    }
}
